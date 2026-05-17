import React, { useState, useMemo } from 'react';
import { PieChart, Pie, Cell, Tooltip as RechartsTooltip, Legend, ResponsiveContainer, BarChart, Bar, XAxis, YAxis, CartesianGrid } from 'recharts';
import { TrendingUp, TrendingDown, ArrowLeftRight } from 'lucide-react';
import { DashboardSection } from '../components/DashboardSection';
import { Card } from '../components/Card';
import { Skeleton } from '../components/Skeleton';
import { EmptyState } from '../components/EmptyState';
import { useGetApiTransaction } from '../api/generated/endpoints';
import { formatCurrency } from '../utils/formatters';
import { isIncomeType, isExpenseType } from '../utils/display-helpers';
import { useCurrencyConvert } from '../hooks/useCurrencyConvert';

const COLORS = ['#FF6B6B', '#4ECDC4', '#45B7D1', '#FFA07A', '#98D8C8', '#F7DC6F', '#BB8FCE', '#F1948A'];

export const Analytics: React.FC = () => {
  const [dateFrom, setDateFrom] = useState<string>(() => {
    const date = new Date();
    date.setMonth(date.getMonth() - 1);
    return date.toISOString().split('T')[0];
  });
  const [dateTo, setDateTo] = useState<string>(new Date().toISOString().split('T')[0]);

  const { convert, selectedCurrencyName } = useCurrencyConvert();

  const transactionsQuery = useGetApiTransaction({
    Limit: 10000,
    FromDate: new Date(dateFrom).toISOString(),
    ToDate: new Date(dateTo).toISOString(),
  });

  const rawTransactions = (Array.isArray(transactionsQuery.data?.data)
    ? transactionsQuery.data?.data
    : Array.isArray((transactionsQuery.data as any)?.items)
    ? (transactionsQuery.data as any).items
    : Array.isArray(transactionsQuery.data)
    ? transactionsQuery.data
    : []) as any[];

  const transactions = [...rawTransactions].sort(
    (a, b) => new Date(b.date ?? 0).getTime() - new Date(a.date ?? 0).getTime()
  );

  const isLoading = transactionsQuery.isLoading;
  const isRefetching = transactionsQuery.isFetching && !isLoading;

  const stats = useMemo(() => {
    let income = 0;
    let expense = 0;
    const byCategory: Record<string, number> = {};
    const timelineMap: Record<string, { income: number; expense: number }> = {};

    transactions.forEach((t: any) => {
      const typeName = t.transactionType?.name;
      const amount = convert(t.amount || 0, t.currency);
      const categoryName = t.category?.name || 'Без категорії';
      const dateStr = t.date
        ? new Date(t.date).toLocaleDateString('uk-UA', { day: 'numeric', month: 'short' })
        : 'Unknown';

      if (!timelineMap[dateStr]) timelineMap[dateStr] = { income: 0, expense: 0 };

      if (isIncomeType(typeName)) {
        income += amount;
        timelineMap[dateStr].income += amount;
      } else if (isExpenseType(typeName)) {
        expense += amount;
        timelineMap[dateStr].expense += amount;
        byCategory[categoryName] = (byCategory[categoryName] || 0) + amount;
      }
    });

    const timeline = Object.entries(timelineMap).map(([date, data]) => ({ date, ...data }));
    return { income, expense, byCategory, timeline };
  }, [transactions, convert, selectedCurrencyName]);

  const pieData = useMemo(() =>
    Object.entries(stats.byCategory)
      .map(([name, value]) => ({ name, value }))
      .sort((a, b) => b.value - a.value),
    [stats.byCategory]
  );

  const net = stats.income - stats.expense;

  if (isLoading) {
    return (
      <div className="space-y-8">
        <Skeleton className="h-8 w-60" />
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
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
    <div className="w-full space-y-8">
      <DashboardSection title="Аналітика транзакцій">
        {/* Date Range */}
        <div className="grid grid-cols-2 gap-3 mb-4">
          <div>
            <label className="block text-xs font-semibold text-[#7a7a7a] mb-1.5">Від</label>
            <input
              type="date"
              value={dateFrom}
              onChange={(e) => setDateFrom(e.target.value)}
              className="w-full px-3 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary text-sm text-[#1d1d1f]"
            />
          </div>
          <div>
            <label className="block text-xs font-semibold text-[#7a7a7a] mb-1.5">До</label>
            <input
              type="date"
              value={dateTo}
              onChange={(e) => setDateTo(e.target.value)}
              className="w-full px-3 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary text-sm text-[#1d1d1f]"
            />
          </div>
        </div>

        {isRefetching && (
          <div className="flex items-center gap-2 text-xs text-primary mb-4">
            <svg className="animate-spin h-3.5 w-3.5" viewBox="0 0 24 24" fill="none">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="3" />
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v4l3-3-3-3v4a8 8 0 00-8 8h4z" />
            </svg>
            Оновлення...
          </div>
        )}

        {/* Summary Cards */}
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <Card className="!bg-green-50 border-green-200">
            <div className="flex justify-between items-start mb-2">
              <span className="text-xs font-semibold text-[#7a7a7a] uppercase tracking-wider">Дохід</span>
              <TrendingUp size={20} className="text-green-500" />
            </div>
            <div className="text-2xl font-bold text-green-600 font-mono">
              {formatCurrency(stats.income, 2)}
            </div>
            <div className="text-xs text-green-700/70 mt-1">{selectedCurrencyName}</div>
          </Card>

          <Card className="!bg-red-50 border-red-200">
            <div className="flex justify-between items-start mb-2">
              <span className="text-xs font-semibold text-[#7a7a7a] uppercase tracking-wider">Витрати</span>
              <TrendingDown size={20} className="text-red-500" />
            </div>
            <div className="text-2xl font-bold text-red-600 font-mono">
              {formatCurrency(stats.expense, 2)}
            </div>
            <div className="text-xs text-red-700/70 mt-1">{selectedCurrencyName}</div>
          </Card>

          <Card className={`border-2 ${net >= 0 ? '!bg-blue-50 border-blue-200' : '!bg-orange-50 border-orange-200'}`}>
            <div className="flex justify-between items-start mb-2">
              <span className="text-xs font-semibold text-[#7a7a7a] uppercase tracking-wider">Баланс</span>
              <ArrowLeftRight size={20} className={net >= 0 ? 'text-blue-500' : 'text-orange-500'} />
            </div>
            <div className={`text-2xl font-bold font-mono ${net >= 0 ? 'text-blue-600' : 'text-orange-600'}`}>
              {net >= 0 ? '+' : ''}{formatCurrency(net, 2)}
            </div>
            <div className="text-xs text-[#7a7a7a] mt-1">{transactions.length} транзакцій</div>
          </Card>
        </div>

        {/* Bar chart: income vs expense timeline */}
        {stats.timeline.length > 0 ? (
          <Card className="flex flex-col" style={{ height: 280 }}>
            <h3 className="text-sm font-semibold text-ink mb-3">Динаміка по днях</h3>
            <div className="flex-1 min-h-0 w-full">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={stats.timeline} margin={{ top: 4, right: 4, left: -20, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f0f0f0" />
                  <XAxis
                    dataKey="date"
                    axisLine={false}
                    tickLine={false}
                    tick={{ fill: '#7a7a7a', fontSize: 11 }}
                    dy={8}
                    interval="preserveStartEnd"
                  />
                  <YAxis
                    axisLine={false}
                    tickLine={false}
                    tick={{ fill: '#7a7a7a', fontSize: 11 }}
                    tickFormatter={(v) => v > 0 ? `${(v / 1000).toFixed(0)}k` : '0'}
                  />
                  <RechartsTooltip
                    formatter={(value) => [`${formatCurrency(Number(value ?? 0), 2)} ${selectedCurrencyName}`, '']}
                    contentStyle={{ borderRadius: '12px', border: '1px solid #f0f0f0', boxShadow: '0 4px 6px -1px rgb(0 0 0 / 0.1)', fontSize: 12 }}
                    cursor={{ fill: '#f5f5f7' }}
                  />
                  <Legend verticalAlign="top" height={28} iconType="circle" wrapperStyle={{ fontSize: 12 }} />
                  <Bar dataKey="income" name="Дохід" fill="#34c759" radius={[4, 4, 0, 0]} maxBarSize={32} />
                  <Bar dataKey="expense" name="Витрати" fill="#ff3b30" radius={[4, 4, 0, 0]} maxBarSize={32} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </Card>
        ) : (
          <EmptyState title="Немає транзакцій" description="За вказаний період не знайдено транзакцій." />
        )}

        {/* Category breakdown */}
        {pieData.length > 0 && (
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
            <Card className="flex flex-col" style={{ height: 320 }}>
              <h3 className="text-sm font-semibold text-ink mb-2">Структура витрат</h3>
              <div className="flex-1 min-h-0 w-full">
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie
                      data={pieData}
                      cx="50%"
                      cy="45%"
                      innerRadius={55}
                      outerRadius={90}
                      paddingAngle={4}
                      dataKey="value"
                    >
                      {pieData.map((_, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <RechartsTooltip
                      formatter={(value) => [`${formatCurrency(Number(value ?? 0), 2)} ${selectedCurrencyName}`, '']}
                      contentStyle={{ borderRadius: '12px', border: '1px solid #f0f0f0', fontSize: 12 }}
                    />
                    <Legend verticalAlign="bottom" height={32} iconType="circle" wrapperStyle={{ fontSize: 11 }} />
                  </PieChart>
                </ResponsiveContainer>
              </div>
            </Card>

            <Card>
              <h3 className="text-sm font-semibold text-ink mb-3">Деталізація витрат</h3>
              <div className="space-y-2 max-h-64 overflow-y-auto pr-1">
                {pieData.map((item, index) => {
                  const pct = stats.expense > 0 ? (item.value / stats.expense) * 100 : 0;
                  return (
                    <div key={item.name}>
                      <div className="flex items-center justify-between mb-0.5">
                        <div className="flex items-center gap-2 min-w-0">
                          <div className="w-2.5 h-2.5 rounded-full shrink-0" style={{ backgroundColor: COLORS[index % COLORS.length] }} />
                          <span className="text-xs text-[#7a7a7a] truncate">{item.name}</span>
                        </div>
                        <span className="font-mono text-xs font-semibold text-ink ml-2 shrink-0">
                          {formatCurrency(item.value, 2)}
                        </span>
                      </div>
                      <div className="h-1 bg-[#f0f0f0] rounded-full overflow-hidden">
                        <div
                          className="h-full rounded-full transition-all"
                          style={{ width: `${pct}%`, backgroundColor: COLORS[index % COLORS.length] }}
                        />
                      </div>
                    </div>
                  );
                })}
              </div>
              <div className="mt-3 pt-3 border-t border-hairline flex justify-between text-xs">
                <span className="text-[#7a7a7a]">Всього витрат</span>
                <span className="font-mono font-bold text-red-500">{formatCurrency(stats.expense, 2)} {selectedCurrencyName}</span>
              </div>
            </Card>
          </div>
        )}
      </DashboardSection>
    </div>
  );
};
