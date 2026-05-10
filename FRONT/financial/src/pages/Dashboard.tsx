import React, { useState, useRef, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useQueryClient, keepPreviousData } from '@tanstack/react-query';
import { DashboardSection } from '../components/DashboardSection';
import { SourceCard } from '../components/SourceCard';
import { AimProgressCard } from '../components/AimProgressCard';
import { TransactionTable } from '../components/TransactionTable';
import { TransactionFilter, type TransactionFilters } from '../components/TransactionFilter';
import { Button } from '../components/Button';
import { Modal } from '../components/Modal';
import { Skeleton } from '../components/Skeleton';
import { EmptyState } from '../components/EmptyState';
import {
  useGetApiSource,
  useGetApiAim,
  useGetApiTransaction,
  usePostApiTransaction,
  useGetApiTransactionType,
  useGetApiCategory,
} from '../api/generated/endpoints';
import {
  calculateTotalAimsProgress
} from '../utils/calculations';
import { AIMS_PREVIEW_COUNT, TRANSACTIONS_PREVIEW_COUNT } from '../utils/constants';
import { useCurrencyConvert } from '../hooks/useCurrencyConvert';
import { useUIStore } from '../store/uiStore';
import { getTransactionTypeLabel } from '../utils/display-helpers';


export const Dashboard: React.FC = () => {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { convert, selectedCurrencyName } = useCurrencyConvert();
  const { expandedAims, setExpandedAims } = useUIStore();

  // Quick-create transaction modal state
  const [isCreateTxOpen, setIsCreateTxOpen] = useState(false);
  const [txForm, setTxForm] = useState<{
    amount: string;
    date: string;
    sourceId: string;
    transactionTypeId: string;
    categoryId: string;
    comment: string;
  }>({
    amount: '',
    date: new Date().toISOString().split('T')[0],
    sourceId: '',
    transactionTypeId: '',
    categoryId: '',
    comment: '',
  });
  const [txErrors, setTxErrors] = useState<{ amount?: string; sourceId?: string; transactionTypeId?: string }>({});

  const [filters, setFilters] = useState<TransactionFilters>({
    SortBy: 'Date',
    SortDescending: true,
  });

  // API Queries
  const sourcesQuery = useGetApiSource();
  const aimsQuery = useGetApiAim();
  const transactionsQuery = useGetApiTransaction(
    { Limit: TRANSACTIONS_PREVIEW_COUNT, ...filters },
    { query: { placeholderData: keepPreviousData } }
  );
  const typesQuery = useGetApiTransactionType();
  const categoriesQuery = useGetApiCategory();

  const createTxMutation = usePostApiTransaction();

  // Loading states
  const isLoading = sourcesQuery.isLoading || aimsQuery.isLoading || transactionsQuery.isLoading;

  // Data
  const sources = (Array.isArray(sourcesQuery.data) ? sourcesQuery.data : []) as any[];
  const aimsRaw = (Array.isArray((aimsQuery.data as any)?.data) ? (aimsQuery.data as any).data : Array.isArray(aimsQuery.data) ? aimsQuery.data : []) as any[];
  const aims = [...aimsRaw].sort((a, b) => (a.priority ?? 0) - (b.priority ?? 0));
  const transactions = (Array.isArray(transactionsQuery.data?.data) ? transactionsQuery.data.data : []) as any[];
  const types = (Array.isArray(typesQuery.data) ? typesQuery.data : []) as any[];
  const categories = (Array.isArray(categoriesQuery.data) ? categoriesQuery.data : []) as any[];

  const totalAmount = sources.reduce((sum: number, source: any) => {
    return sum + convert(source.amount ?? 0, source.currency);
  }, 0);
  const totalAimsProgress = calculateTotalAimsProgress(aims);

  const scrollRef = useRef<HTMLDivElement>(null);
  const [canScrollRight, setCanScrollRight] = useState(false);
  const [canScrollLeft, setCanScrollLeft] = useState(false);
  
  const checkScroll = () => {
    if (scrollRef.current) {
      const { scrollLeft, scrollWidth, clientWidth } = scrollRef.current;
      setCanScrollRight(scrollLeft + clientWidth < scrollWidth - 1);
      setCanScrollLeft(scrollLeft > 0);
    }
  };

  useEffect(() => {
    checkScroll();
    window.addEventListener('resize', checkScroll);
    return () => window.removeEventListener('resize', checkScroll);
  }, [sources]);

  const handleScrollRight = () => {
    if (scrollRef.current) {
      scrollRef.current.scrollBy({ left: 300, behavior: 'smooth' });
    }
  };

  const handleScrollLeft = () => {
    if (scrollRef.current) {
      scrollRef.current.scrollBy({ left: -300, behavior: 'smooth' });
    }
  };

  const handleOpenCreateTxFromSource = (sourceId: number) => {
    const incomeType = types.find((t: any) => {
      const name = String(t?.name ?? '').toLowerCase();
      return name === 'income' || name === 'надходження' || name === 'поповнення' || name === 'дохід';
    });

    setTxForm((prev) => ({
      ...prev,
      sourceId: String(sourceId),
      transactionTypeId: incomeType ? String(incomeType.id) : prev.transactionTypeId,
    }));
    setTxErrors((prev) => ({ ...prev, sourceId: undefined, transactionTypeId: undefined }));
    setIsCreateTxOpen(true);
  };

  const handleCreateTx = async (e: React.FormEvent) => {
    e.preventDefault();
    const errors: { amount?: string; sourceId?: string; transactionTypeId?: string } = {};
    if (!txForm.amount || Number(txForm.amount) <= 0) errors.amount = 'Сума має бути більшою за 0';
    if (!txForm.sourceId) errors.sourceId = 'Виберіть джерело';
    if (!txForm.transactionTypeId) errors.transactionTypeId = 'Виберіть тип';
    setTxErrors(errors);
    if (Object.keys(errors).length > 0) return;

    try {
      const source = sources.find(s => s.id === Number(txForm.sourceId));
      await createTxMutation.mutateAsync({
        data: {
          amount: Number(txForm.amount),
          date: txForm.date,
          sourceId: Number(txForm.sourceId),
          transactionTypeId: Number(txForm.transactionTypeId),
          categoryId: txForm.categoryId ? Number(txForm.categoryId) : null,
          comment: txForm.comment || '',
          currencyId: source?.currency?.id,
        },
      });
      setIsCreateTxOpen(false);
      setTxForm({ amount: '', date: new Date().toISOString().split('T')[0], sourceId: '', transactionTypeId: '', categoryId: '', comment: '' });
      setTxErrors({});
      queryClient.invalidateQueries({ queryKey: ['/api/Transaction'] });
      queryClient.invalidateQueries({ queryKey: ['/api/Source'] });
    } catch (err: any) {
      console.error('Error creating transaction:', err?.response?.data || err);
      // If there are validation errors, we can show them
      if (err?.response?.data?.errors) {
        alert(JSON.stringify(err.response.data.errors, null, 2));
      }
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-8">
        <div className="space-y-4">
          <Skeleton className="h-8 w-64" />
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
            <Skeleton className="h-40 rounded-2xl" />
            <Skeleton className="h-40 rounded-2xl" />
            <Skeleton className="h-40 rounded-2xl" />
            <Skeleton className="h-40 rounded-2xl" />
          </div>
        </div>
        <div className="space-y-4">
          <Skeleton className="h-8 w-56" />
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-4">
            <Skeleton className="h-36 rounded-2xl" />
            <Skeleton className="h-36 rounded-2xl" />
            <Skeleton className="h-36 rounded-2xl" />
          </div>
        </div>
        <div className="space-y-4">
          <Skeleton className="h-8 w-72" />
          <Skeleton className="h-80 rounded-2xl" />
        </div>
      </div>
    );
  }

  return (
    <div className="w-full">
      {/* Sources Section */}
      <DashboardSection
        title="Мої джерела"
        action={<Button onClick={() => navigate('/sources')}>Відкрити джерела</Button>}
      >
        <div className="relative group">
          <div 
            ref={scrollRef}
            onScroll={checkScroll}
            className="flex overflow-x-auto gap-4 pb-4 snap-x hide-scrollbar items-stretch"
            style={{ scrollbarWidth: 'none', msOverflowStyle: 'none' }}
          >
            {/* Total Card */}
            <div className="bg-gradient-to-br from-surface-tile-1 to-ink text-white rounded-2xl p-6 shadow-xl flex flex-col justify-between min-w-[280px] w-[280px] shrink-0 snap-start">
              <div className="flex justify-between items-center mb-4">
                <span className="text-sm font-medium opacity-80">Загальна сума</span>
                <span className="bg-white/10 text-white border border-white/20 px-2 py-1 rounded text-xs font-semibold">
                  {selectedCurrencyName}
                </span>
              </div>
              <div className="font-mono text-4xl font-semibold my-4">{totalAmount.toFixed(2)}</div>
              <button
                className="mt-2 bg-white/10 text-white border border-white/20 py-2 px-4 rounded-lg text-sm font-medium hover:bg-white/15 transition-colors self-start"
                onClick={() => navigate('/sources')}
              >
                Всі джерела →
              </button>
            </div>

            {/* Source Cards */}
            {sources.map((source) => (
              <div key={source.id} className="snap-start shrink-0 min-w-[280px] w-[280px] flex items-stretch">
                <div className="w-full h-full" onClick={() => handleOpenCreateTxFromSource(source.id)}>
                  <SourceCard
                    source={source as any}
                    compact={false}
                  />
                </div>
              </div>
            ))}
          </div>

          {/* Scroll Arrows */}
          {canScrollLeft && (
            <button
              onClick={handleScrollLeft}
              className="absolute left-4 top-1/2 -translate-y-1/2 h-12 w-12 bg-white border border-hairline rounded-full shadow-lg flex items-center justify-center text-primary hover:bg-[#fafafc] hover:scale-105 transition-all z-10 opacity-0 group-hover:opacity-100 pointer-events-none group-hover:pointer-events-auto"
              aria-label="Гортати назад"
            >
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <polyline points="15 18 9 12 15 6" />
              </svg>
            </button>
          )}

          {canScrollRight && (
            <button
              onClick={handleScrollRight}
              className="absolute right-4 top-1/2 -translate-y-1/2 h-12 w-12 bg-white border border-hairline rounded-full shadow-lg flex items-center justify-center text-primary hover:bg-[#fafafc] hover:scale-105 transition-all z-10 opacity-0 group-hover:opacity-100 pointer-events-none group-hover:pointer-events-auto"
              aria-label="Гортати далі"
            >
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <polyline points="9 18 15 12 9 6" />
              </svg>
            </button>
          )}
        </div>

        {sources.length === 0 && (
          <EmptyState
            title="Немає джерел"
            description="Створіть перше джерело на сторінці управління або через швидкий доступ."
          />
        )}
      </DashboardSection>

      {/* Aims Section */}
      <DashboardSection
        title="Цілі"
        subtitle="Відсортовано за пріоритетом · натисніть на ціль для редагування"
        action={<Button onClick={() => navigate('/aims')}>Відкрити цілі</Button>}
      >
        <div className="flex flex-col gap-4">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-4">
            {aims.slice(0, AIMS_PREVIEW_COUNT).map((aim) => (
              <div
                key={aim.id}
                className="cursor-pointer"
                onClick={() => navigate(`/aims?editId=${aim.id}`)}
              >
                <AimProgressCard aim={aim} />
              </div>
            ))}
          </div>

          {/* Total Progress */}
          {aims.length > 0 && (
            <div className="bg-white border border-hairline rounded-lg p-5 flex items-center gap-4">
              <span className="text-sm font-semibold text-ink min-w-[180px]">
                Загальний прогрес по всім цілям
              </span>
              <div className="flex-1 h-1.5 bg-hairline rounded-full overflow-hidden">
                <div
                  className="h-full bg-gradient-to-r from-primary to-primary-focus transition-all duration-300"
                  style={{ width: `${Math.min(totalAimsProgress, 100)}%` }}
                />
              </div>
              <span className="text-sm font-semibold text-primary min-w-[50px] text-right">
                {totalAimsProgress.toFixed(1)}%
              </span>
            </div>
          )}

          {/* Expanded Aims */}
          {expandedAims && aims.length > AIMS_PREVIEW_COUNT && (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-4">
              {aims.slice(AIMS_PREVIEW_COUNT).map((aim) => (
                <div
                  key={aim.id}
                  className="cursor-pointer"
                  onClick={() => navigate(`/aims?editId=${aim.id}`)}
                >
                  <AimProgressCard aim={aim} />
                </div>
              ))}
            </div>
          )}

          {aims.length > AIMS_PREVIEW_COUNT && (
            <button
              className="w-full bg-white border border-hairline px-5 py-3 rounded-lg text-primary font-medium hover:bg-[#f5f5f7] transition-colors"
              onClick={() => setExpandedAims(!expandedAims)}
            >
              {expandedAims ? 'Згорнути' : 'Показати всі цілі'}
            </button>
          )}

          {aims.length === 0 && (
            <EmptyState
              title="Немає цілей"
              description="Додайте першу ціль і почніть відслідковувати прогрес."
            />
          )}
        </div>
      </DashboardSection>

      {/* Transactions Section */}
      <DashboardSection
        title="Останні транзакції"
        action={
          <div className="flex gap-2">
            <Button
              onClick={() => {
                setTxForm({
                  amount: '',
                  date: new Date().toISOString().split('T')[0],
                  sourceId: '',
                  transactionTypeId: '',
                  categoryId: '',
                  comment: '',
                });
                setTxErrors({});
                setIsCreateTxOpen(true);
              }}
            >
              + Транзакція
            </Button>
            <Button variant="secondary" onClick={() => navigate('/transactions')}>Всі транзакції</Button>
          </div>
        }
      >
        <TransactionFilter
          filters={filters}
          onFilterChange={setFilters}
          onClearFilters={() => setFilters({ SortBy: 'Date', SortDescending: true })}
        />
        <TransactionTable transactions={transactions as any} />
        <Link
          to="/transactions"
          className="inline-block mt-4 text-primary font-medium hover:text-primary-focus transition-colors text-sm"
        >
          Переглянути всі →
        </Link>
      </DashboardSection>

      {/* Quick Create Transaction Modal */}
      <Modal
        isOpen={isCreateTxOpen}
        title="Нова транзакція"
        onClose={() => setIsCreateTxOpen(false)}
        size="md"
      >
        <form onSubmit={handleCreateTx} className="space-y-4">
          <div>
            <label className="block text-sm font-semibold text-ink mb-2">Тип</label>
            <select
              value={txForm.transactionTypeId}
              onChange={(e) => {
                setTxForm({ ...txForm, transactionTypeId: e.target.value });
                if (txErrors.transactionTypeId) setTxErrors((p) => ({ ...p, transactionTypeId: undefined }));
              }}
              className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]"
            >
              <option value="">Виберіть тип</option>
              {types.map((t: any) => (
                <option key={t.id} value={t.id}>{getTransactionTypeLabel(t.name).label}</option>
              ))}
            </select>
            {txErrors.transactionTypeId && <p className="mt-1 text-xs text-red-500">{txErrors.transactionTypeId}</p>}
          </div>

          <div>
            <label className="block text-sm font-semibold text-ink mb-2">Сума</label>
            <input
              type="number"
              value={txForm.amount}
              onChange={(e) => {
                setTxForm({ ...txForm, amount: e.target.value });
                if (txErrors.amount) setTxErrors((p) => ({ ...p, amount: undefined }));
              }}
              placeholder="0.00"
              step="0.01"
              min="0.01"
              className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]"
            />
            {txErrors.amount && <p className="mt-1 text-xs text-red-500">{txErrors.amount}</p>}
          </div>

          <div>
            <label className="block text-sm font-semibold text-ink mb-2">Дата</label>
            <input
              type="date"
              value={txForm.date}
              onChange={(e) => setTxForm({ ...txForm, date: e.target.value })}
              className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]"
            />
          </div>

          <div>
            <label className="block text-sm font-semibold text-ink mb-2">Джерело</label>
            <select
              value={txForm.sourceId}
              onChange={(e) => {
                setTxForm({ ...txForm, sourceId: e.target.value });
                if (txErrors.sourceId) setTxErrors((p) => ({ ...p, sourceId: undefined }));
              }}
              className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]"
            >
              <option value="">Виберіть джерело</option>
              {sources.map((s: any) => (
                <option key={s.id} value={s.id}>{s.name}</option>
              ))}
            </select>
            {txErrors.sourceId && <p className="mt-1 text-xs text-red-500">{txErrors.sourceId}</p>}
          </div>

          <div>
            <label className="block text-sm font-semibold text-ink mb-2">Категорія (опціонально)</label>
            <select
              value={txForm.categoryId}
              onChange={(e) => setTxForm({ ...txForm, categoryId: e.target.value })}
              className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]"
            >
              <option value="">Без категорії</option>
              {categories.map((c: any) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-semibold text-ink mb-2">Коментар (опціонально)</label>
            <textarea
              value={txForm.comment}
              onChange={(e) => setTxForm({ ...txForm, comment: e.target.value })}
              placeholder="Додайте коментар..."
              rows={2}
              className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f] resize-none"
            />
          </div>

          <div className="flex gap-3 pt-2">
            <Button variant="secondary" onClick={() => setIsCreateTxOpen(false)} type="button">
              Скасувати
            </Button>
            <Button type="submit" isLoading={createTxMutation.isPending}>
              Створити
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
};
