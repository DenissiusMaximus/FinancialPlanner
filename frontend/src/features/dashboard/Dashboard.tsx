import React from 'react';
import { useGetSourcesSummaryQuery, useGetTransactionsQuery, useGetAimsQuery } from '../../store/apiSlice';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../../components/ui/Card';
import { Wallet, TrendingUp, TrendingDown, Target } from 'lucide-react';
import { formatCurrency, toSafeNumber } from '../../utils/number';

export function Dashboard() {
  const { data: sourceSummary, isLoading: sourcesLoading } = useGetSourcesSummaryQuery();
  const { data: transactionsResult, isLoading: txLoading } = useGetTransactionsQuery({ limit: 5, offset: 0 });
  const { data: aims } = useGetAimsQuery({});

  // /api/Source/summary returns { total, sources: [] } — not a plain array
  const sources = sourceSummary?.sources ?? [];
  const totalBalance = sourceSummary?.total ?? 0;

  // /api/Transaction returns { data: [], meta: {} } — paginated result
  const transactions = transactionsResult?.data ?? [];

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center">
        <h1 className="text-3xl font-bold tracking-tight text-slate-900">Dashboard</h1>
        <div className="mt-4 sm:mt-0 space-x-2">
          {/* Action buttons could go here */}
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        <Card className="bg-gradient-to-br from-primary-600 to-primary-800 text-white border-transparent">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-primary-100">Total Balance</CardTitle>
            <Wallet className="h-4 w-4 text-primary-100" />
          </CardHeader>
          <CardContent>
            <div className="text-3xl font-bold">
              ${formatCurrency(totalBalance)}
            </div>
            <p className="text-xs text-primary-200 mt-1">Across all accounts</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-slate-500">Active Goals</CardTitle>
            <Target className="h-4 w-4 text-slate-400" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{aims?.length || 0}</div>
            <p className="text-xs text-slate-500 mt-1">Goals tracking in progress</p>
          </CardContent>
        </Card>
        
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-slate-500">Subscriptions</CardTitle>
            <Wallet className="h-4 w-4 text-slate-400" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold text-indigo-600">Active</div>
            <p className="text-xs text-slate-500 mt-1">Manage recurring payments</p>
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-7">
        <Card className="lg:col-span-4">
          <CardHeader>
            <CardTitle>Recent Transactions</CardTitle>
            <CardDescription>Your latest financial activity.</CardDescription>
          </CardHeader>
          <CardContent>
            {txLoading ? (
              <div className="text-center py-4 text-slate-500">Loading transactions...</div>
            ) : transactions?.length > 0 ? (
              <div className="space-y-4">
                {transactions.map((tx) => {
                  const amount = toSafeNumber(tx.amount);

                  return (
                  <div key={tx.id} className="flex items-center justify-between border-b border-slate-100 pb-4 last:border-0 last:pb-0">
                    <div className="flex items-center space-x-4">
                      <div className={`p-2 rounded-full ${amount < 0 ? 'bg-red-100 text-red-600' : 'bg-emerald-100 text-emerald-600'}`}>
                        {amount < 0 ? <TrendingDown className="h-4 w-4" /> : <TrendingUp className="h-4 w-4" />}
                      </div>
                      <div>
                        <p className="text-sm font-medium text-slate-900">{tx.comment || 'Transaction'}</p>
                        <p className="text-xs text-slate-500">{new Date(tx.date).toLocaleDateString()}</p>
                      </div>
                    </div>
                    <div className={`font-medium ${amount < 0 ? 'text-slate-900' : 'text-emerald-600'}`}>
                      {amount < 0 ? '-' : '+'}${formatCurrency(Math.abs(amount))}
                    </div>
                  </div>
                  );
                })}
              </div>
            ) : (
              <div className="text-center py-8 text-slate-500 border-2 border-dashed border-slate-200 rounded-lg">
                No recent transactions
              </div>
            )}
          </CardContent>
        </Card>

        <Card className="lg:col-span-3">
          <CardHeader>
            <CardTitle>Sources Summary</CardTitle>
            <CardDescription>Balances by account.</CardDescription>
          </CardHeader>
          <CardContent>
            {sourcesLoading ? (
              <div className="text-center py-4 text-slate-500">Loading sources...</div>
            ) : sources?.length > 0 ? (
              <div className="space-y-4">
                {sources.map((source) => (
                  <div key={source.id} className="flex items-center justify-between">
                    <div className="flex items-center space-x-3">
                      <div className="w-2 h-2 rounded-full bg-primary-500" />
                      <span className="text-sm font-medium text-slate-700">{source.name}</span>
                    </div>
                    <span className="text-sm font-semibold text-slate-900">
                      ${formatCurrency(source.amount, { minimumFractionDigits: 2 })}
                    </span>
                  </div>
                ))}
              </div>
            ) : (
              <div className="text-center py-8 text-slate-500 border-2 border-dashed border-slate-200 rounded-lg">
                No sources defined
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
