import React, { useState, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { PieChart, Pie, Cell, Tooltip as RechartsTooltip, Legend, ResponsiveContainer, BarChart, Bar, XAxis, YAxis, CartesianGrid } from 'recharts';
import { TrendingUp, TrendingDown, CheckCircle2, AlertCircle } from 'lucide-react';
import { DashboardSection } from '../components/DashboardSection';
import { Card } from '../components/Card';
import { Skeleton } from '../components/Skeleton';
import { EmptyState } from '../components/EmptyState';
import { customInstance } from '../api/custom-instance';
import { useGetApiTransaction } from '../api/generated/endpoints';
import { formatCurrency } from '../utils/formatters';
import { isIncomeType, isExpenseType } from '../utils/display-helpers';
import type { PlannedTransactionDto } from '../types/generated';

export const Analytics: React.FC = () => {
  const [dateFrom, setDateFrom] = useState<string>(() => {
    const date = new Date();
    date.setMonth(date.getMonth() - 1);
    return date.toISOString().split('T')[0];
  });
  const [dateTo, setDateTo] = useState<string>(new Date().toISOString().split('T')[0]);

  // API Queries
  const transactionsQuery = useGetApiTransaction({
    Limit: 10000,
    FromDate: new Date(dateFrom).toISOString(),
    ToDate: new Date(dateTo).toISOString(),
  });

  const plannedQuery = useQuery({
    queryKey: ['/api/PlannedTransaction'],
    queryFn: () => customInstance<PlannedTransactionDto[]>({ url: '/api/PlannedTransaction', method: 'GET' })
  });

  const transactions = (Array.isArray(transactionsQuery.data?.data)
    ? transactionsQuery.data?.data
    : Array.isArray(transactionsQuery.data)
    ? transactionsQuery.data
    : []) as any[];
  
  const plannedRaw = plannedQuery.data as any;
  const planned = (Array.isArray(plannedRaw?.data) ? plannedRaw.data : Array.isArray(plannedRaw) ? plannedRaw : []) as PlannedTransactionDto[];

  const isLoading = transactionsQuery.isLoading || plannedQuery.isLoading;

  const filteredTransactions = transactions;

  // Calculate totals by type
  const stats = useMemo(() => {
    let income = 0;
    let expense = 0;
    let transfer = 0;
    const byCategory: Record<string, number> = {};
    const timelineMap: Record<string, { income: number; expense: number }> = {};

    filteredTransactions.forEach((t: any) => {
      const typeName = t.transactionType?.name;
      const amount = t.amount || 0;
      const categoryName = t.category?.name || 'Без категорії';
      const dateStr = t.date ? new Date(t.date).toLocaleDateString('uk-UA', { day: 'numeric', month: 'short' }) : 'Unknown';

      if (!timelineMap[dateStr]) timelineMap[dateStr] = { income: 0, expense: 0 };

      if (isIncomeType(typeName)) {
        income += amount;
        timelineMap[dateStr].income += amount;
      } else if (isExpenseType(typeName)) {
        expense += amount;
        timelineMap[dateStr].expense += amount;
        byCategory[categoryName] = (byCategory[categoryName] || 0) + amount;
      } else {
        transfer += amount;
      }
    });

    const timeline = Object.entries(timelineMap).map(([date, data]) => ({ date, ...data }));

    return { income, expense, transfer, byCategory, timeline };
  }, [filteredTransactions]);

  // Calculate monthly planned
  const plannedStats = useMemo(() => {
    let monthlyIncome = 0;
    let monthlyExpense = 0;

    planned.forEach((p) => {
      const typeName = p.transactionType?.name;
      const amount = p.amount || 0;
      let multiplier = 1;

      if (p.frequency) {
        const unit = (p.frequency.intervalUnit?.name || '').toLowerCase();
        const val = p.frequency.intervalValue || 1;

        if (unit.includes('day') || unit.includes('день') || unit.includes('щодня')) multiplier = 30 / val;
        else if (unit.includes('week') || unit.includes('тиж') || unit.includes('щотижня')) multiplier = 4.33 / val;
        else if (unit.includes('month') || unit.includes('місяц') || unit.includes('щомісяця')) multiplier = 1 / val;
        else if (unit.includes('year') || unit.includes('рік') || unit.includes('щорічно')) multiplier = (1 / 12) / val;
      }

      if (isIncomeType(typeName)) monthlyIncome += amount * multiplier;
      else if (isExpenseType(typeName)) monthlyExpense += amount * multiplier;
    });

    return { monthlyIncome, monthlyExpense };
  }, [planned]);

  const pieData = useMemo(() => {
    return Object.entries(stats.byCategory)
      .map(([name, value]) => ({ name, value }))
      .sort((a, b) => b.value - a.value);
  }, [stats.byCategory]);

  const COLORS = ['#FF6B6B', '#4ECDC4', '#45B7D1', '#FFA07A', '#98D8C8', '#F7DC6F', '#BB8FCE', '#F1948A'];

  const net = stats.income - stats.expense;

  if (isLoading) {
    return (
      <div className="space-y-8">
        <Skeleton className="h-8 w-60" />
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <Skeleton className="h-32 rounded-2xl" />
          <Skeleton className="h-32 rounded-2xl" />
          <Skeleton className="h-32 rounded-2xl" />
        </div>
        <Skeleton className="h-72 rounded-2xl" />
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Skeleton className="h-40 rounded-2xl" />
          <Skeleton className="h-40 rounded-2xl" />
        </div>
      </div>
    );
  }

  return (
    <div className="w-full">
      {/* Date Range Filter */}
      <DashboardSection title="Дохід та витрати">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
          <div>
            <label className="block text-sm font-semibold text-ink mb-2">Від</label>
            <input
              type="date"
              value={dateFrom}
              onChange={(e) => setDateFrom(e.target.value)}
              className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]"
            />
          </div>
          <div>
            <label className="block text-sm font-semibold text-ink mb-2">До</label>
            <input
              type="date"
              value={dateTo}
              onChange={(e) => setDateTo(e.target.value)}
              className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]"
            />
          </div>
        </div>

        {/* Summary Cards */}
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-8">
          {/* Income */}
          <Card className="!bg-green-50 border-green-200">
            <div className="flex justify-between items-start mb-2">
              <span className="text-sm font-semibold text-[#7a7a7a] uppercase tracking-wider">Дохід (Факт / План)</span>
              <TrendingUp size={24} className="text-green-500" />
            </div>
            <div className="text-3xl font-bold text-green-600 font-mono mb-1">
              {formatCurrency(stats.income, 2)}
            </div>
            <div className="text-xs text-green-700/70 font-medium">
              Очікувано за місяць: {formatCurrency(plannedStats.monthlyIncome, 0)}
            </div>
          </Card>

          {/* Expense */}
          <Card className="!bg-red-50 border-red-200">
            <div className="flex justify-between items-start mb-2">
              <span className="text-sm font-semibold text-[#7a7a7a] uppercase tracking-wider">Витрати (Факт / План)</span>
              <TrendingDown size={24} className="text-red-500" />
            </div>
            <div className="text-3xl font-bold text-red-600 font-mono mb-1">
              {formatCurrency(stats.expense, 2)}
            </div>
            <div className="text-xs text-red-700/70 font-medium">
              Очікувано за місяць: {formatCurrency(plannedStats.monthlyExpense, 0)}
            </div>
          </Card>

          {/* Net */}
          <Card className={`!border-2 ${net >= 0 ? '!bg-blue-50 border-blue-200' : '!bg-orange-50 border-orange-200'}`}>
            <div className="flex justify-between items-start mb-2">
              <span className="text-sm font-semibold text-[#7a7a7a] uppercase tracking-wider">Чистий результат</span>
              {net >= 0 ? <CheckCircle2 size={24} className="text-blue-500" /> : <AlertCircle size={24} className="text-orange-500" />}
            </div>
            <div className={`text-3xl font-bold font-mono ${net >= 0 ? 'text-blue-600' : 'text-orange-600'}`}>
              {net >= 0 ? '+' : ''}{formatCurrency(net, 2)}
            </div>
          </Card>
        </div>

        {/* Timeline Chart */}
        <Card className="mb-8 flex flex-col h-[350px]">
          <h3 className="text-lg font-semibold text-ink mb-4">Динаміка грошового потоку</h3>
          {stats.timeline.length > 0 ? (
            <div className="flex-1 min-h-0 w-full">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={stats.timeline} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f0f0f0" />
                  <XAxis 
                    dataKey="date" 
                    axisLine={false}
                    tickLine={false}
                    tick={{ fill: '#7a7a7a', fontSize: 12 }}
                    dy={10}
                  />
                  <YAxis 
                    axisLine={false}
                    tickLine={false}
                    tick={{ fill: '#7a7a7a', fontSize: 12 }}
                    tickFormatter={(value) => value > 0 ? `${(value/1000).toFixed(0)}k` : '0'}
                  />
                  <RechartsTooltip 
                    formatter={(value) => formatCurrency(Number(value ?? 0), 2)}
                    contentStyle={{ borderRadius: '12px', border: '1px solid #f0f0f0', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
                    cursor={{ fill: '#f5f5f7' }}
                  />
                  <Legend verticalAlign="top" height={36} iconType="circle" />
                  <Bar dataKey="income" name="Дохід" fill="#34c759" radius={[4, 4, 0, 0]} maxBarSize={40} />
                  <Bar dataKey="expense" name="Витрати" fill="#ff3b30" radius={[4, 4, 0, 0]} maxBarSize={40} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          ) : (
            <div className="flex-1 flex items-center justify-center">
              <EmptyState
                title="Немає транзакцій"
                description="Додайте транзакції, щоб побачити графік."
              />
            </div>
          )}
        </Card>

        {/* Breakdown by Category and Chart */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <Card className="flex flex-col h-[400px]">
            <h3 className="text-lg font-semibold text-ink mb-4">Структура витрат</h3>
            {pieData.length > 0 ? (
              <div className="flex-1 min-h-0 w-full">
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie
                      data={pieData}
                      cx="50%"
                      cy="50%"
                      innerRadius={60}
                      outerRadius={100}
                      paddingAngle={5}
                      dataKey="value"
                    >
                      {pieData.map((_, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <RechartsTooltip 
                      formatter={(value) => formatCurrency(Number(value ?? 0), 2)}
                      contentStyle={{ borderRadius: '12px', border: '1px solid #f0f0f0', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)' }}
                    />
                    <Legend verticalAlign="bottom" height={36} iconType="circle" />
                  </PieChart>
                </ResponsiveContainer>
              </div>
            ) : (
              <div className="flex-1 flex items-center justify-center">
                <EmptyState
                  title="Немає витрат"
                  description="За цей період не знайдено витрат."
                />
              </div>
            )}
          </Card>

          <Card className="flex flex-col h-[400px]">
            <h3 className="text-lg font-semibold text-ink mb-4">Деталізація за категоріями</h3>
            <div className="flex-1 overflow-y-auto pr-2">
              {pieData.length > 0 ? (
                <div className="space-y-4">
                  {pieData.map((item, index) => (
                    <div key={item.name} className="flex items-center justify-between">
                      <div className="flex items-center gap-3">
                        <div 
                          className="w-3 h-3 rounded-full" 
                          style={{ backgroundColor: COLORS[index % COLORS.length] }} 
                        />
                        <span className="text-sm font-medium text-[#7a7a7a]">{item.name}</span>
                      </div>
                      <span className="font-mono font-semibold text-ink">
                        {formatCurrency(item.value, 2)}
                      </span>
                    </div>
                  ))}
                </div>
              ) : (
                <div className="h-full flex items-center justify-center">
                  <span className="text-[#7a7a7a] text-sm">Немає даних</span>
                </div>
              )}
            </div>
          </Card>
        </div>

        {/* Stats */}
        <div className="mt-6 bg-[#f5f5f7] rounded-lg p-4 text-sm text-[#7a7a7a]">
          <div className="flex flex-wrap justify-between gap-2">
            <span>Всього транзакцій: {filteredTransactions.length}</span>
            <span>Період: {dateFrom} – {dateTo}</span>
          </div>
        </div>
      </DashboardSection>

      {/* Detailed Transaction Breakdown */}
      <DashboardSection title="Статистика типів">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Card>
            <div className="space-y-3">
              <h4 className="font-semibold text-ink mb-4">Дохідні операції</h4>
              <div className="flex items-end justify-between">
                <span className="text-sm text-[#7a7a7a]">Кількість:</span>
                <span className="text-2xl font-bold text-green-600">
                  {filteredTransactions.filter((t: any) => isIncomeType(t.transactionType?.name)).length}
                </span>
              </div>
              <div className="flex items-end justify-between">
                <span className="text-sm text-[#7a7a7a]">Сума:</span>
                <span className="text-xl font-mono font-semibold text-green-600">
                  {formatCurrency(stats.income, 2)}
                </span>
              </div>
            </div>
          </Card>

          <Card>
            <div className="space-y-3">
              <h4 className="font-semibold text-ink mb-4">Витратні операції</h4>
              <div className="flex items-end justify-between">
                <span className="text-sm text-[#7a7a7a]">Кількість:</span>
                <span className="text-2xl font-bold text-red-600">
                  {filteredTransactions.filter((t: any) => isExpenseType(t.transactionType?.name)).length}
                </span>
              </div>
              <div className="flex items-end justify-between">
                <span className="text-sm text-[#7a7a7a]">Сума:</span>
                <span className="text-xl font-mono font-semibold text-red-600">
                  {formatCurrency(stats.expense, 2)}
                </span>
              </div>
            </div>
          </Card>
        </div>

        {/* Average Transaction */}
        <Card className="mt-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <span className="text-xs font-semibold text-[#7a7a7a] uppercase tracking-wider mb-1 block">
                Середня сума доходу
              </span>
              <div className="text-2xl font-mono font-bold text-green-600">
                {formatCurrency(
                  stats.income / Math.max(
                    filteredTransactions.filter((t: any) => isIncomeType(t.transactionType?.name)).length,
                    1
                  ),
                  2
                )}
              </div>
            </div>
            <div>
              <span className="text-xs font-semibold text-[#7a7a7a] uppercase tracking-wider mb-1 block">
                Середня сума витрат
              </span>
              <div className="text-2xl font-mono font-bold text-red-600">
                {formatCurrency(
                  stats.expense / Math.max(
                    filteredTransactions.filter((t: any) => isExpenseType(t.transactionType?.name)).length,
                    1
                  ),
                  2
                )}
              </div>
            </div>
          </div>
        </Card>
      </DashboardSection>
    </div>
  );
};
