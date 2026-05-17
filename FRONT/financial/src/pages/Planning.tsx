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
import { useCurrencyStore } from '../store/currencyStore';
import { useCurrencyConvert } from '../hooks/useCurrencyConvert';
import type { PlannedTransactionDto, AimDto } from '../types/generated';

export const Planning: React.FC = () => {
  const [monthsToForecast, setMonthsToForecast] = useState<number>(6);
  const selectedCurrency = useCurrencyStore((s) => s.selectedCurrency);
  const { convert } = useCurrencyConvert();

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
  const planned = (Array.isArray(plannedRaw?.data) ? plannedRaw.data : Array.isArray(plannedRaw) ? plannedRaw : []) as PlannedTransactionDto[];

  // 1. Calculate Expected Monthly Savings Rate & Category Expenses
  const { monthlyIncome, monthlyExpense, monthlySavings, expenseByCategory } = useMemo(() => {
    let inc = 0;
    let exp = 0;
    const catMap: Record<string, number> = {};

    planned.forEach((p) => {
      const typeName = p.transactionType?.name;
      const amount = convert(p.amount || 0, p.currency);
      let multiplier = 1;

      if (p.frequency) {
        const unit = (p.frequency.intervalUnit?.name || '').toLowerCase();
        const val = p.frequency.intervalValue || 1;

        if (unit.includes('day') || unit.includes('день') || unit.includes('щодня')) multiplier = 30 / val;
        else if (unit.includes('week') || unit.includes('тиж') || unit.includes('щотижня')) multiplier = 4.33 / val;
        else if (unit.includes('month') || unit.includes('місяц') || unit.includes('щомісяця')) multiplier = 1 / val;
        else if (unit.includes('year') || unit.includes('рік') || unit.includes('щорічно')) multiplier = (1 / 12) / val;
      }

      if (isIncomeType(typeName)) {
        inc += amount * multiplier;
      } else if (isExpenseType(typeName)) {
        const mExp = amount * multiplier;
        exp += mExp;
        const catName = p.category?.name || 'Без категорії';
        catMap[catName] = (catMap[catName] || 0) + mExp;
      }
    });

    return { 
      monthlyIncome: inc, 
      monthlyExpense: exp, 
      monthlySavings: inc - exp,
      expenseByCategory: catMap
    };
  }, [planned]);

  // 2. Waterfall Forecast Logic
  const forecastAims = useMemo(() => {
    if (monthlySavings <= 0) return []; // Cannot forecast if losing money or break-even

    // Get active aims and sort by priority (1 is highest)
    const activeAims = aims
      .filter(a => !a.isClosed && (a.amount ?? 0) > (a.progress?.collectedAmount ?? 0))
      .sort((a, b) => (a.priority ?? 999) - (b.priority ?? 999));

    let currentAccumulatedSavings = monthlySavings * monthsToForecast;
    let currentMonthOffset = 0;
    
    return activeAims.map((aim) => {
      const target = convert(aim.amount ?? 0, aim.currency);
      const collected = convert(aim.progress?.collectedAmount ?? 0, aim.currency);
      const remainingToCollect = target - collected;

      // How many months it takes just for this goal given full savings power
      const monthsNeededForThisGoal = remainingToCollect / monthlySavings;
      const willBeAchievedInMonth = currentMonthOffset + Math.ceil(monthsNeededForThisGoal);

      // Will it be achieved within the forecasted period?
      const willAchieveInForecast = remainingToCollect <= currentAccumulatedSavings;
      
      let forecastedAdditionalCollection = 0;
      if (willAchieveInForecast) {
        forecastedAdditionalCollection = remainingToCollect;
        currentAccumulatedSavings -= remainingToCollect;
        currentMonthOffset += monthsNeededForThisGoal;
      } else {
        forecastedAdditionalCollection = currentAccumulatedSavings;
        currentAccumulatedSavings = 0;
      }

      // Calculate future date
      const achievementDate = new Date();
      achievementDate.setMonth(achievementDate.getMonth() + willBeAchievedInMonth);

      return {
        ...aim,
        target,
        collected,
        remainingToCollect,
        forecastedAdditionalCollection,
        willAchieveInForecast,
        willBeAchievedInMonth,
        achievementDateStr: achievementDate.toLocaleDateString('uk-UA', { month: 'long', year: 'numeric' }),
        newTotalCollected: collected + forecastedAdditionalCollection,
        newProgressPercentage: Math.min(((collected + forecastedAdditionalCollection) / target) * 100, 100)
      };
    });
  }, [aims, monthlySavings, monthsToForecast]);

  const targetDate = useMemo(() => {
    const d = new Date();
    d.setMonth(d.getMonth() + monthsToForecast);
    return d.toLocaleDateString('uk-UA', { month: 'long', year: 'numeric' });
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
              <div className={`text-2xl sm:text-3xl font-mono font-bold ${monthlySavings > 0 ? 'text-green-600' : 'text-red-500'}`}>
                {formatCurrency(monthlySavings * monthsToForecast, 0)} {selectedCurrency}
              </div>
            </div>
            <div className="grid grid-cols-2 gap-3 pt-2">
              <div>
                <div className="text-xs font-semibold text-[#7a7a7a] uppercase mb-1">Всього Доходів</div>
                <div className="text-lg font-mono font-bold text-ink">
                  {formatCurrency(monthlyIncome * monthsToForecast, 0)} {selectedCurrency}
                </div>
              </div>
              <div>
                <div className="text-xs font-semibold text-[#7a7a7a] uppercase mb-1">Всього Витрат</div>
                <div className="text-lg font-mono font-bold text-ink">
                  {formatCurrency(monthlyExpense * monthsToForecast, 0)} {selectedCurrency}
                </div>
              </div>
            </div>
          </Card>

          {/* Status Alert */}
          {monthlySavings <= 0 && (
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
          {monthlySavings > 0 && (
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
                            Залишилось зібрати: <span className="font-mono font-medium text-ink">{formatCurrency(aim.remainingToCollect, 0)}</span> {selectedCurrency}
                          </div>
                        </div>
                        
                        <div className="self-start">
                          {aim.willAchieveInForecast ? (
                            <div className="flex items-center gap-2 text-green-700 bg-green-50 px-3 py-1.5 rounded-lg border border-green-200">
                              <CheckCircle2 size={16} />
                              <span className="text-sm font-semibold">Досягнуто у {aim.achievementDateStr}</span>
                            </div>
                          ) : (
                            <div className="flex items-center gap-2 text-primary bg-primary/10 px-3 py-1.5 rounded-lg border border-primary/20">
                              <Calendar size={16} />
                              <span className="text-sm font-semibold">Очікується у {aim.achievementDateStr}</span>
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
                          <span>Зібрано: {formatCurrency(aim.collected, 0)} {selectedCurrency}</span>
                          <span>Додасться: +{formatCurrency(aim.forecastedAdditionalCollection, 0)} {selectedCurrency}</span>
                          <span>Ціль: {formatCurrency(aim.target, 0)} {selectedCurrency}</span>
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
                        {formatCurrency(amount * monthsToForecast, 0)} {selectedCurrency}
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
                  {formatCurrency(monthlyExpense * monthsToForecast, 0)} {selectedCurrency}
                </span>
              </div>
            )}
          </Card>
        </div>

      </div>
    </div>
  );
};
