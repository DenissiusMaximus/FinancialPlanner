import React, { useState, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Target, CheckCircle2, Calendar, AlertCircle } from 'lucide-react';
import { Card } from '../components/Card';
import { Skeleton } from '../components/Skeleton';
import { EmptyState } from '../components/EmptyState';
import { customInstance } from '../api/custom-instance';
import { useGetApiAim } from '../api/generated/endpoints';
import { formatCurrency } from '../utils/formatters';
import { isIncomeType, isExpenseType } from '../utils/display-helpers';
import { useCurrencyConvert } from '../hooks/useCurrencyConvert';
import { PhantomTransactionsBlock } from '../components/PhantomTransactionsBlock';
import { PhantomTransactionModal } from '../components/PhantomTransactionModal';
import { usePhantomTransactions } from '../hooks/usePhantomTransactions';
import type { PhantomTransaction } from '../hooks/usePhantomTransactions';
import type { PlannedTransactionDto, AimDto } from '../types/generated';

export const Planning: React.FC = () => {
  const [monthsToForecast, setMonthsToForecast] = useState<number>(6);
  const [selectedMonthIndex, setSelectedMonthIndex] = useState<number>(0);
  const { convert, selectedCurrencyName } = useCurrencyConvert();

  const { phantoms, addPhantom, editPhantom, deletePhantom, togglePhantom } = usePhantomTransactions();
  const [isPhantomModalOpen, setIsPhantomModalOpen] = useState(false);
  const [editingPhantom, setEditingPhantom] = useState<PhantomTransaction | null>(null);

  // API Queries
  const aimsQuery = useGetApiAim();
  const plannedQuery = useQuery({
    queryKey: ['/api/PlannedTransaction'],
    queryFn: () => customInstance<PlannedTransactionDto[]>({ url: '/api/PlannedTransaction', method: 'GET' })
  });

  const isLoading = aimsQuery.isLoading || plannedQuery.isLoading;

  const aimsRaw = aimsQuery.data as any;
  const aims = (Array.isArray(aimsRaw?.data) ? aimsRaw.data : Array.isArray(aimsRaw) ? aimsRaw : []) as AimDto[];
  
  const plannedRaw = plannedQuery.data as any;
  const planned = (
    Array.isArray(plannedRaw?.data) ? plannedRaw.data :
    Array.isArray(plannedRaw?.items) ? plannedRaw.items :
    Array.isArray(plannedRaw) ? plannedRaw : []
  ) as PlannedTransactionDto[];

  // 1. Calculate per-month equivalents and per-period totals
  const { monthsSavings, monthsIncome, monthsExpense, periodIncome, periodExpense, periodSavings, expenseByCategory } = useMemo(() => {
    const monthsNet: number[] = new Array(monthsToForecast).fill(0);
    const monthsIncomeArr: number[] = new Array(monthsToForecast).fill(0);
    const monthsExpenseArr: number[] = new Array(monthsToForecast).fill(0);
    const catMap: Record<string, number> = {};

    const now = new Date();
    const startOfToday = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    // removed unused intermediate day counters

    // Convert frequency to daily rate using basic unit lengths and intervalValue.
    // Units: day=1, week=7, month=30, year=365. intervalValue multiplies the unit.
    const getDailyRate = (p: PlannedTransactionDto) => {
      const amount = convert(p.amount || 0, p.currency);
      const isOneTime =
        !p.frequency ||
        (!!p.frequency.intervalValue && p.frequency.intervalValue >= 9999) ||
        (p.frequency?.name && /one|одно|однораз|once/i.test(p.frequency.name || ''));

      if (isOneTime) return { dailyRate: 0, isOneTime: true, amount };

      const unitName = (p.frequency?.intervalUnit?.name || '').toString().toLowerCase();
      const val = p.frequency?.intervalValue || 1;
      let unitDays = 30; // default month
      if (unitName.includes('day') || unitName.includes('день') || unitName.includes('щод')) unitDays = 1;
      else if (unitName.includes('week') || unitName.includes('тиж')) unitDays = 7;
      else if (unitName.includes('month') || unitName.includes('міся')) unitDays = 30;
      else if (unitName.includes('year') || unitName.includes('рік') || unitName.includes('річ')) unitDays = 365;

      const intervalDays = unitDays * (val || 1);
      const dailyRate = intervalDays > 0 ? amount / intervalDays : 0;
      return { dailyRate, isOneTime: false, amount };
    };

    // Precompute days for each month in the horizon: month 0 uses remaining days, others full month days
    const daysInMonths: number[] = [];
    for (let i = 0; i < monthsToForecast; i++) {
      const year = now.getFullYear() + Math.floor((now.getMonth() + i) / 12);
      const month = (now.getMonth() + i) % 12;
      const days = new Date(year, month + 1, 0).getDate();
      if (i === 0) {
        const daysRemaining = (new Date(year, month + 1, 0).getDate() - now.getDate()) + 1;
        daysInMonths.push(daysRemaining);
      } else {
        daysInMonths.push(days);
      }
    }

    // helper: parse startDate strings robustly (accept ISO or dd.MM.yyyy)
    const parseDateSafe = (s?: string | null) => {
      if (!s) return null;
      
      let safeStr = s;
      // Fix for Safari: replace "YYYY-MM-DD HH:mm:ss" with "YYYY-MM-DDTHH:mm:ss"
      safeStr = safeStr.trim().replace(' ', 'T');
      // Fix for Safari: truncate fractional seconds to 3 digits (e.g., .1234567 -> .123)
      safeStr = safeStr.replace(/(\.\d{3})\d+/, '$1');

      let d = new Date(safeStr);
      if (!isNaN(d.getTime())) return d;
      
      d = new Date(s);
      if (!isNaN(d.getTime())) return d;
      
      // try dd.MM.yyyy
      const m = /^([0-3]?\d)\.([0-1]?\d)\.([0-9]{4})$/.exec(s);
      if (m) {
        const day = Number(m[1]);
        const month = Number(m[2]) - 1;
        const year = Number(m[3]);
        return new Date(year, month, day);
      }
      return null;
    };

    const debugArr: any[] = [];
    const allPlanned = [...planned, ...phantoms.filter(p => p.isEnabled)];
    
    // Build month-by-month net amounts using daily rates
    allPlanned.forEach((p) => {
      const typeName = p.transactionType?.name;
      const { dailyRate, isOneTime, amount } = getDailyRate(p);
      const perMonthContribs: number[] = new Array(monthsToForecast).fill(0);
      const sign = isIncomeType(typeName) ? 1 : isExpenseType(typeName) ? -1 : 0;

      if (!isOneTime && sign !== 0) {
        // recurring: allocate dailyRate * daysInMonths[i]
        let totalForPeriod = 0;
        const startDate = parseDateSafe(p.startDate) ;
        for (let i = 0; i < monthsToForecast; i++) {
          // compute month start/end for this horizon month
          const year = now.getFullYear() + Math.floor((now.getMonth() + i) / 12);
          const month = (now.getMonth() + i) % 12;
          const monthEnd = new Date(year, month + 1, 0, 23, 59, 59, 999);

          // if there's a configured startDate and it is after the end of this month, skip allocation
          if (startDate && startDate > monthEnd) {
            perMonthContribs[i] = 0;
            continue;
          }

          const contrib = dailyRate * daysInMonths[i] * sign;
          monthsNet[i] += contrib;
          if (sign > 0) monthsIncomeArr[i] += contrib;
          else if (sign < 0) monthsExpenseArr[i] += Math.abs(contrib);
          perMonthContribs[i] = contrib;
          totalForPeriod += (dailyRate * daysInMonths[i]);
        }
        if (isExpenseType(typeName)) {
          const catName = p.category?.name || 'Без категорії';
          catMap[catName] = (catMap[catName] || 0) + totalForPeriod;
        }
        debugArr.push({
          id: p.id,
          name: p.name,
          type: typeName,
          amount: amount,
          currency: p.currency,
          isOneTime: false,
          dailyRate,
          perMonthContribs,
          totalForPeriod,
          frequency: p.frequency?.name || `${p.frequency?.intervalValue} x ${p.frequency?.intervalUnit?.name}`,
          startDate: p.startDate,
          phantomId: (p as any).phantomId,
        });
      } else if (isOneTime) {
        if (!p.startDate) return;
        const start = parseDateSafe(p.startDate);
        if (!start) return;
        // if start is earlier than today and within current month but before now, skip
        if (start < startOfToday && start.getMonth() === now.getMonth() && start.getFullYear() === now.getFullYear()) {
          return;
        }
        const monthIndex = (start.getFullYear() - now.getFullYear()) * 12 + (start.getMonth() - now.getMonth());
        if (monthIndex < 0 || monthIndex >= monthsToForecast) return;
        monthsNet[monthIndex] += amount * sign;
        if (sign > 0) monthsIncomeArr[monthIndex] += amount;
        else if (sign < 0) monthsExpenseArr[monthIndex] += amount;
        perMonthContribs[monthIndex] = amount * sign;
        debugArr.push({
          id: p.id,
          name: p.name,
          type: typeName,
          amount: amount,
          currency: p.currency,
          isOneTime: true,
          dailyRate: 0,
          perMonthContribs,
          totalForPeriod: amount,
          frequency: 'One-time',
          startDate: p.startDate,
          phantomId: (p as any).phantomId,
        });
        if (isExpenseType(typeName)) {
          const catName = p.category?.name || 'Без категорії';
          catMap[catName] = (catMap[catName] || 0) + amount;
        }
      }
    });

    // monthsNet holds net contributions per month (positive income, negative expenses)
    const periodIncFinal = monthsIncomeArr.reduce((s, v) => s + v, 0);
    const periodExpFinal = monthsExpenseArr.reduce((s, v) => s + v, 0);
    const periodSavingsFinal = periodIncFinal - periodExpFinal;

    // sort debug entries by absolute total (largest first)
    debugArr.sort((a, b) => Math.abs((b.totalForPeriod || 0)) - Math.abs((a.totalForPeriod || 0)));

    return {
      monthsSavings: monthsNet,
      monthsIncome: monthsIncomeArr,
      monthsExpense: monthsExpenseArr,
      periodIncome: periodIncFinal,
      periodExpense: periodExpFinal,
      periodSavings: periodSavingsFinal,
      expenseByCategory: catMap,
    };
  }, [planned, phantoms, convert, monthsToForecast, selectedCurrencyName]);

  // 2. Waterfall Forecast Logic
  const forecastAims = useMemo(() => {
    // If period savings are non-positive, return empty
    if (periodSavings <= 0) return [];

    // Prepare active aims ordered by priority
    const activeAims = aims
      .filter(a => !a.isClosed && (a.amount ?? 0) > (a.progress?.collectedAmount ?? 0))
      .sort((a, b) => (a.priority ?? 999) - (b.priority ?? 999));

    // Clone remaining amounts per aim
    const remainingMap = activeAims.map((aim) => ({
      id: aim.id,
      aim,
      target: convert(aim.amount ?? 0, aim.currency),
      collected: convert(aim.progress?.collectedAmount ?? 0, aim.currency),
      remaining: convert(aim.amount ?? 0, aim.currency) - convert(aim.progress?.collectedAmount ?? 0, aim.currency),
      allocated: 0,
      achievedMonth: null as number | null,
    }));

    // Simulate month-by-month allocation using monthsSavings (array of net savings per month)
    for (let m = 0; m < monthsToForecast; m++) {
      let available = monthsSavings[m] ?? 0;
      if (available <= 0) continue;
      for (const r of remainingMap) {
        if (r.remaining <= 0) continue;
        const take = Math.min(r.remaining, available);
        r.allocated += take;
        r.remaining -= take;
        available -= take;
        if (r.remaining <= 0 && r.achievedMonth === null) r.achievedMonth = m;
        if (available <= 0) break;
      }
    }

    // Map results back to aims
    const now = new Date();
    return remainingMap.map((r) => {
      const willAchieveInForecast = r.remaining <= 0;
      // determine which month index to show (if achieved within forecast, show that month; otherwise show last forecast month)
      const showIndex = r.achievedMonth !== null && r.achievedMonth !== undefined ? r.achievedMonth : Math.max(0, monthsToForecast - 1);
      const totalMonthsFromNow = now.getMonth() + showIndex;
      const showYear = now.getFullYear() + Math.floor(totalMonthsFromNow / 12);
      const showMonth = totalMonthsFromNow % 12;
      // last day of that month
      const achievementDate = new Date(showYear, showMonth + 1, 0);
      return {
        ...r.aim,
        target: r.target,
        collected: r.collected,
        remainingToCollect: r.target - r.collected,
        forecastedAdditionalCollection: r.allocated,
        willAchieveInForecast,
        willBeAchievedInMonth: r.achievedMonth ?? -1,
        achievementDateStr: achievementDate.toLocaleDateString('uk-UA', { day: 'numeric', month: 'long', year: 'numeric' }),
        newTotalCollected: r.collected + r.allocated,
        newProgressPercentage: Math.min(((r.collected + r.allocated) / r.target) * 100, 100),
      };
    });
  }, [aims, monthsSavings, monthsToForecast, convert, selectedCurrencyName, periodSavings]);

  const targetDate = useMemo(() => {
    const now = new Date();
    const end = new Date(now.getFullYear(), now.getMonth() + monthsToForecast, 0); // last day of the final month in horizon
    return end.toLocaleDateString('uk-UA', { day: 'numeric', month: 'long', year: 'numeric' });
  }, [monthsToForecast]);

  if (isLoading) {
    return (
      <div className="space-y-8">
        <Skeleton className="h-8 w-60" />
        <Skeleton className="h-20 w-full rounded-2xl" />
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Skeleton className="h-32 rounded-2xl" />
          <Skeleton className="h-32 rounded-2xl" />
        </div>
        <Skeleton className="h-72 rounded-2xl" />
      </div>
    );
  }

  return (
    <div className="w-full space-y-6">
      <div className="flex flex-col gap-3 lg:flex-row lg:items-end lg:justify-between">
        <h1 className="text-3xl font-semibold text-ink">Прогнозування</h1>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        {/* Left Column: Controls & Overall Stats */}
        <div className="lg:col-span-2 space-y-6">
          <Card className="bg-white border border-hairline p-6">
            <div className="flex justify-between items-center mb-4">
              <label className="text-sm font-semibold text-ink">
                Горизонт планування: <span className="text-primary">{monthsToForecast} міс.</span> (до {targetDate})
              </label>
            </div>
            
            <input 
              type="range" 
              min="1" 
              max="60" 
              value={monthsToForecast} 
              onChange={(e) => setMonthsToForecast(Number(e.target.value))}
              className="w-full h-2 bg-[#f0f0f0] rounded-lg appearance-none cursor-pointer accent-primary"
            />
            <div className="flex justify-between text-xs text-[#7a7a7a] mt-2 mb-6">
              <span>1 міс</span>
              <span>1 рік</span>
              <span>3 роки</span>
              <span>5 років</span>
            </div>

            {/* Savings highlight */}
            <div className="pt-4 border-t border-[#f0f0f0]">
              <div className="text-xs font-semibold text-[#7a7a7a] uppercase mb-1">Накопичення</div>
              <div className={`text-2xl sm:text-3xl font-mono font-bold ${periodSavings > 0 ? 'text-green-600' : 'text-red-500'}`}>
                {formatCurrency(periodSavings, 0)} {selectedCurrencyName}
              </div>
            </div>
            <div className="grid grid-cols-2 gap-3 pt-4 mt-4 border-t border-hairline">
              <div>
                <div className="text-xs font-semibold text-[#7a7a7a] uppercase mb-1">Всього Доходів</div>
                <div className="text-lg font-mono font-bold text-ink">
                  {formatCurrency(periodIncome, 0)} {selectedCurrencyName}
                </div>
              </div>
              <div>
                <div className="text-xs font-semibold text-[#7a7a7a] uppercase mb-1">Всього Витрат</div>
                <div className="text-lg font-mono font-bold text-ink">
                  {formatCurrency(periodExpense, 0)} {selectedCurrencyName}
                </div>
              </div>
            </div>
          </Card>

          <PhantomTransactionsBlock 
            phantoms={phantoms}
            togglePhantom={togglePhantom}
            deletePhantom={deletePhantom}
            onAddClick={() => { setEditingPhantom(null); setIsPhantomModalOpen(true); }}
            onEditClick={(p) => { setEditingPhantom(p); setIsPhantomModalOpen(true); }}
          />

          <PhantomTransactionModal 
            isOpen={isPhantomModalOpen}
            onClose={() => setIsPhantomModalOpen(false)}
            editingTransaction={editingPhantom}
            onSave={(t) => {
              if (editingPhantom) {
                editPhantom(editingPhantom.phantomId, t);
              } else {
                addPhantom(t);
              }
            }}
          />

          <Card className="bg-white border border-hairline p-5">
            <div className="flex items-center gap-2 mb-4">
              <Calendar size={20} className="text-primary" />
              <h3 className="text-lg font-semibold text-ink">Місячний прогноз</h3>
            </div>
            
            {/* Horizontal Scroll for Months */}
            <div 
              className="flex overflow-x-auto gap-3 pb-2 -mx-1 px-1" 
              style={{ scrollbarWidth: 'none', msOverflowStyle: 'none' }}
            >
              {monthsSavings.map((amt, i) => {
                const d = new Date();
                const monthIndex = d.getMonth() + i;
                const year = d.getFullYear() + Math.floor(monthIndex / 12);
                const month = monthIndex % 12;
                
                const monthName = new Date(year, month, 1).toLocaleDateString('uk-UA', { month: 'short', year: 'numeric' });
                const isSelected = i === selectedMonthIndex;
                const isPositive = amt > 0;
                
                return (
                  <button
                    key={i}
                    onClick={() => setSelectedMonthIndex(i)}
                    className={`flex-shrink-0 flex flex-col items-center justify-center p-3 rounded-2xl border transition-all min-w-[100px] ${
                      isSelected 
                        ? 'border-primary bg-primary text-white shadow-md' 
                        : `bg-white hover:bg-[#f9f9fb] ${isPositive ? 'border-green-200' : 'border-red-200'}`
                    }`}
                  >
                    <span className={`text-xs font-semibold capitalize mb-1 ${isSelected ? 'text-white/90' : 'text-[#7a7a7a]'}`}>
                      {monthName}
                    </span>
                    <span className={`font-mono font-bold text-sm ${isSelected ? 'text-white' : (isPositive ? 'text-green-600' : 'text-red-500')}`}>
                      {amt > 0 ? '+' : ''}{formatCurrency(amt, 0)}
                    </span>
                  </button>
                );
              })}
            </div>

            {/* Selected Month Inline Detail */}
            {monthsSavings[selectedMonthIndex] !== undefined && (
              <div className="mt-6 pt-5 border-t border-hairline animate-in fade-in slide-in-from-top-2 duration-300">
                <div className="flex items-center justify-between mb-4">
                  <h4 className="text-sm font-semibold text-ink">
                    Деталі за {new Date(
                      new Date().getFullYear() + Math.floor((new Date().getMonth() + selectedMonthIndex) / 12),
                      (new Date().getMonth() + selectedMonthIndex) % 12,
                      1
                    ).toLocaleDateString('uk-UA', { month: 'long', year: 'numeric' })}
                  </h4>
                </div>
                
                <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
                  <div className="bg-[#f9f9fb] p-4 rounded-xl border border-hairline/50">
                    <div className="text-[10px] text-[#7a7a7a] font-sans uppercase font-bold leading-none mb-2">Доходи</div>
                    <div className="text-green-600 font-mono font-bold text-lg">
                      +{formatCurrency(monthsIncome[selectedMonthIndex] || 0, 0)}
                    </div>
                  </div>
                  <div className="bg-[#f9f9fb] p-4 rounded-xl border border-hairline/50">
                    <div className="text-[10px] text-[#7a7a7a] font-sans uppercase font-bold leading-none mb-2">Витрати</div>
                    <div className="text-red-500 font-mono font-bold text-lg">
                      -{formatCurrency(monthsExpense[selectedMonthIndex] || 0, 0)}
                    </div>
                  </div>
                  <div className={`col-span-2 md:col-span-1 bg-[#f9f9fb] p-4 rounded-xl border border-hairline/50`}>
                    <div className="text-[10px] text-[#7a7a7a] font-sans uppercase font-bold leading-none mb-2">Баланс</div>
                    <div className={`font-mono font-bold text-lg ${monthsSavings[selectedMonthIndex] > 0 ? 'text-green-600' : 'text-red-500'}`}>
                      {monthsSavings[selectedMonthIndex] > 0 ? '+' : ''}{formatCurrency(monthsSavings[selectedMonthIndex], 0)}
                    </div>
                  </div>
                </div>
              </div>
            )}
          </Card>

          {/* Status Alert */}
          {periodSavings <= 0 && (
            <div className="rounded-xl border border-red-200 bg-red-50 p-4 flex gap-3 items-start text-red-700">
              <AlertCircle className="shrink-0 mt-0.5" size={20} />
              <div>
                <h4 className="font-semibold text-sm">Негативний або нульовий потенціал заощаджень</h4>
                <p className="text-sm mt-1">
                  Ваші планові витрати перевищують планові доходи. Ми не можемо побудувати прогноз досягнення цілей. Будь ласка, перегляньте свої планові транзакції.
                </p>
              </div>
            </div>
          )}

          {/* Forecast List */}
          {periodSavings > 0 && (
            <div>
              <h3 className="text-lg font-semibold text-ink mb-4 flex items-center gap-2">
                <Target size={20} className="text-primary" />
                Досягнення цілей
              </h3>
              
              {forecastAims.length > 0 ? (
                <div className="grid gap-4 grid-cols-1">
                  {forecastAims.map((aim) => (
                    <Card key={aim.id} className="border border-hairline p-5">
                      <div className="flex flex-col gap-3 mb-4">
                        <div className="flex-1">
                          <div className="flex items-center gap-2 mb-1 flex-wrap">
                            <span className="text-xs font-bold text-[#7a7a7a] bg-[#f5f5f7] px-2 py-0.5 rounded-md">
                              Пріоритет {aim.priority}
                            </span>
                            <h4 className="text-base sm:text-lg font-semibold text-ink">{aim.name}</h4>
                          </div>
                          <div className="text-sm text-[#7a7a7a]">
                            Залишилось зібрати: <span className="font-mono font-medium text-ink">{formatCurrency(aim.remainingToCollect, 0)}</span> {selectedCurrencyName}
                          </div>
                        </div>
                        
                        <div className="self-start">
                          {aim.willAchieveInForecast ? (
                            <div className="flex items-center gap-2 text-green-700 bg-green-50 px-3 py-1.5 rounded-lg border border-green-200">
                              <CheckCircle2 size={16} />
                              <span className="text-sm font-semibold">Досягнуто до {aim.achievementDateStr}</span>
                            </div>
                          ) : (
                            <div className="flex items-center gap-2 text-primary bg-primary/10 px-3 py-1.5 rounded-lg border border-primary/20">
                              <Calendar size={16} />
                              <span className="text-sm font-semibold">Очікується до {aim.achievementDateStr}</span>
                            </div>
                          )}
                        </div>
                      </div>

                      {/* Progress Bar Container */}
                      <div className="space-y-2">
                        <div className="flex justify-between text-xs font-semibold text-ink">
                          <span>Зараз: {Number(aim.progress?.completionPercentage ?? 0).toFixed(1)}%</span>
                          <span className="text-primary">Прогноз: {aim.newProgressPercentage.toFixed(1)}%</span>
                        </div>
                        <div className="h-3 w-full bg-[#f0f0f0] rounded-full overflow-hidden relative">
                          {/* Forecasted additional progress (extends from 0 to new percentage) */}
                          <div 
                            className="absolute top-0 left-0 h-full bg-primary/30 z-10 transition-all duration-500"
                            style={{ width: `${aim.newProgressPercentage}%` }}
                          />
                           {/* Base actual progress (on top of the forecasted one) */}
                           <div 
                            className="absolute top-0 left-0 h-full bg-primary z-20 transition-all duration-500"
                            style={{ width: `${Math.min(Number(aim.progress?.completionPercentage ?? 0), 100)}%` }}
                          />
                        </div>
                        <div className="flex justify-between text-[11px] text-[#7a7a7a]">
                          <span>Зібрано: {formatCurrency(aim.collected, 0)} {selectedCurrencyName}</span>
                          <span>Додасться: +{formatCurrency(aim.forecastedAdditionalCollection, 0)} {selectedCurrencyName}</span>
                          <span>Ціль: {formatCurrency(aim.target, 0)} {selectedCurrencyName}</span>
                        </div>
                      </div>
                    </Card>
                  ))}
                </div>
              ) : (
                <EmptyState 
                  title="Немає активних цілей"
                  description="Створіть цілі на сторінці 'Цілі', щоб побачити прогноз їх досягнення."
                />
              )}
            </div>
          )}
        </div>

        {/* Right Column: Category Forecast */}
        <div className="lg:col-span-1">
          <Card className="bg-white border border-hairline p-5 h-full flex flex-col">
            <h3 className="text-lg font-semibold text-ink mb-1">Прогноз витрат</h3>
            <p className="text-xs text-[#7a7a7a] mb-5">
              Скільки ви витратите за {monthsToForecast} міс. за кожною категорією (на основі ваших планів)
            </p>

            <div className="flex-1 overflow-y-auto pr-2 space-y-4">
              {Object.keys(expenseByCategory).length > 0 ? (
                Object.entries(expenseByCategory)
                  .sort(([, a], [, b]) => b - a)
                  .map(([cat, amount]) => (
                    <div key={cat} className="flex justify-between items-center border-b border-hairline/50 pb-2 last:border-0 last:pb-0">
                      <span className="text-sm font-medium text-[#7a7a7a]">{cat}</span>
                      <span className="font-mono font-semibold text-ink">
                        {formatCurrency(amount, 0)} {selectedCurrencyName}
                      </span>
                    </div>
                  ))
              ) : (
                <div className="flex items-center justify-center h-32 text-sm text-[#7a7a7a]">
                  Немає планових витрат
                </div>
              )}
            </div>
            
            {Object.keys(expenseByCategory).length > 0 && (
              <div className="mt-4 pt-4 border-t border-hairline flex justify-between items-center">
                <span className="text-sm font-semibold text-ink">Всього витрат</span>
                <span className="text-lg font-mono font-bold text-red-500">
                  {formatCurrency(periodExpense, 0)} {selectedCurrencyName}
                </span>
              </div>
            )}
          </Card>
        </div>

      </div>
    </div>
  );
};
