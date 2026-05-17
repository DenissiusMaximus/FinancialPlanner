import React, { useState, useMemo } from 'react';
import { TrendingUp, TrendingDown, ArrowLeftRight } from 'lucide-react';
import { DashboardSection } from '../components/DashboardSection';
import { Card } from '../components/Card';
import { Skeleton } from '../components/Skeleton';
import { useGetApiTransaction } from '../api/generated/endpoints';
import { formatCurrency } from '../utils/formatters';
import { isIncomeType, isExpenseType } from '../utils/display-helpers';
import { useCurrencyConvert } from '../hooks/useCurrencyConvert';

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

  const categoryStats = useMemo(() => {
    let totalIncome = 0;
    let totalExpense = 0;
    const stats: Record<string, { name: string; income: number; expense: number; net: number }> = {};

    transactions.forEach((t: any) => {
      const typeName = t.transactionType?.name;
      const amount = convert(t.amount || 0, t.currency);
      const categoryName = t.category?.name || 'Без категорії';

      if (!stats[categoryName]) {
        stats[categoryName] = { name: categoryName, income: 0, expense: 0, net: 0 };
      }

      if (isIncomeType(typeName)) {
        stats[categoryName].income += amount;
        stats[categoryName].net += amount;
        totalIncome += amount;
      } else if (isExpenseType(typeName)) {
        stats[categoryName].expense += amount;
        stats[categoryName].net -= amount;
        totalExpense += amount;
      }
    });

    const categoriesArray = Object.values(stats).sort((a, b) => b.name.localeCompare(a.name));

    return {
      categories: categoriesArray,
      total: {
        income: totalIncome,
        expense: totalExpense,
        net: totalIncome - totalExpense
      }
    };
  }, [transactions, convert, selectedCurrencyName]);

  const net = categoryStats.total.net;

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
              {formatCurrency(categoryStats.total.income, 2)}
            </div>
            <div className="text-xs text-green-700/70 mt-1">{selectedCurrencyName}</div>
          </Card>

          <Card className="!bg-red-50 border-red-200">
            <div className="flex justify-between items-start mb-2">
              <span className="text-xs font-semibold text-[#7a7a7a] uppercase tracking-wider">Витрати</span>
              <TrendingDown size={20} className="text-red-500" />
            </div>
            <div className="text-2xl font-bold text-red-600 font-mono">
              {formatCurrency(categoryStats.total.expense, 2)}
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

        {/* Category Table */}
        <Card className="p-0 overflow-hidden mt-8">
          <div className="overflow-x-auto">
            <table className="w-full text-sm text-left">
              <thead className="text-xs text-[#7a7a7a] uppercase bg-[#f5f5f7] border-b border-hairline">
                <tr>
                  <th className="px-4 py-3 font-semibold tracking-wider">Категорія</th>
                  <th className="px-4 py-3 text-right font-semibold tracking-wider">Доходи</th>
                  <th className="px-4 py-3 text-right font-semibold tracking-wider">Витрати</th>
                  <th className="px-4 py-3 text-right font-semibold tracking-wider">Разом</th>
                </tr>
              </thead>
              <tbody>
                {categoryStats.categories.length > 0 ? (
                  categoryStats.categories.map((cat) => (
                    <tr key={cat.name} className="border-b border-[#f0f0f0] hover:bg-[#fafafc] transition-colors">
                      <td className="px-4 py-3 font-medium text-ink">{cat.name}</td>
                      <td className="px-4 py-3 text-right text-green-600 font-mono">
                        {cat.income > 0 ? formatCurrency(cat.income, 2) : '—'}
                      </td>
                      <td className="px-4 py-3 text-right text-red-500 font-mono">
                        {cat.expense > 0 ? formatCurrency(cat.expense, 2) : '—'}
                      </td>
                      <td className="px-4 py-3 text-right font-mono font-semibold">
                        <span className={cat.net > 0 ? 'text-green-600' : cat.net < 0 ? 'text-red-500' : 'text-ink'}>
                          {cat.net > 0 ? '+' : ''}{formatCurrency(cat.net, 2)}
                        </span>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={4} className="px-4 py-8 text-center text-[#7a7a7a]">
                      Немає транзакцій за цей період
                    </td>
                  </tr>
                )}
              </tbody>
              <tfoot className="bg-[#f5f5f7] font-bold border-t-2 border-hairline">
                <tr>
                  <td className="px-4 py-3 text-ink">Всі разом</td>
                  <td className="px-4 py-3 text-right text-green-600 font-mono">
                    {formatCurrency(categoryStats.total.income, 2)}
                  </td>
                  <td className="px-4 py-3 text-right text-red-500 font-mono">
                    {formatCurrency(categoryStats.total.expense, 2)}
                  </td>
                  <td className="px-4 py-3 text-right font-mono">
                    <span className={categoryStats.total.net > 0 ? 'text-green-600' : categoryStats.total.net < 0 ? 'text-red-500' : 'text-ink'}>
                      {categoryStats.total.net > 0 ? '+' : ''}{formatCurrency(categoryStats.total.net, 2)}
                    </span>
                  </td>
                </tr>
              </tfoot>
            </table>
          </div>
        </Card>
      </DashboardSection>
    </div>
  );
};
