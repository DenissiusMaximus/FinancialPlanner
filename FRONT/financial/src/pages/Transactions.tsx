import React, { useState } from 'react';
import { useQueryClient, keepPreviousData } from '@tanstack/react-query';
import { DashboardSection } from '../components/DashboardSection';
import { TransactionTable } from '../components/TransactionTable';
import { TransactionFilter, type TransactionFilters } from '../components/TransactionFilter';
import { Button } from '../components/Button';
import { Modal } from '../components/Modal';
import { ConfirmModal } from '../components/ConfirmModal';
import { Skeleton } from '../components/Skeleton';
import { EmptyState } from '../components/EmptyState';
import {
  useGetApiTransaction,
  usePostApiTransaction,
  usePatchApiTransactionId,
  useDeleteApiTransactionId,
  useGetApiCategory,
  useGetApiSource,
  useGetApiTransactionType,
} from '../api/generated/endpoints';
import { getTransactionTypeLabel } from '../utils/display-helpers';
import { getLocalDatetime } from '../utils/formatters';

export const Transactions: React.FC = () => {
  const queryClient = useQueryClient();
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [transactionToDelete, setTransactionToDelete] = useState<number | null>(null);
  const [filters, setFilters] = useState<TransactionFilters>({
    SortBy: 'Date',
    SortDescending: true,
  });
  const [createErrors, setCreateErrors] = useState<{ amount?: string; date?: string; sourceId?: string; transactionTypeId?: string }>({});
  const [formData, setFormData] = useState<{
    amount: string;
    date: string;
    sourceId: string;
    transactionTypeId: string;
    categoryId: string;
    comment: string;
  }>({
    amount: '',
    date: getLocalDatetime(),
    sourceId: '',
    transactionTypeId: '',
    categoryId: '',
    comment: '',
  });

  // Build API query params — pass filters to server
  const queryParams = {
    Limit: 200,
    ...filters,
  };

  // API Queries
  const transactionsQuery = useGetApiTransaction(
    queryParams,
    { query: { placeholderData: keepPreviousData } }
  );
  const categoriesQuery = useGetApiCategory();
  const sourcesQuery = useGetApiSource();
  const typesQuery = useGetApiTransactionType();

  // Mutations
  const createMutation = usePostApiTransaction();
  const updateMutation = usePatchApiTransactionId();
  const deleteMutation = useDeleteApiTransactionId();

  const rawTransactions = (Array.isArray(transactionsQuery.data?.data)
    ? transactionsQuery.data!.data
    : Array.isArray((transactionsQuery.data as any)?.items)
    ? (transactionsQuery.data as any).items
    : Array.isArray(transactionsQuery.data)
    ? transactionsQuery.data
    : []) as any[];

  // Client-side filter by type (backend may not support it reliably)
  const transactions = rawTransactions
    .filter((t: any) =>
      !filters.TransactionTypeId ||
      Number(t.transactionType?.id) === Number(filters.TransactionTypeId)
    )
    // Client-side sort by full datetime
    .sort((a: any, b: any) => {
      if (filters.SortBy === 'Amount') {
        return filters.SortDescending
          ? (b.amount ?? 0) - (a.amount ?? 0)
          : (a.amount ?? 0) - (b.amount ?? 0);
      }
      const da = new Date(a.date ?? 0).getTime();
      const db = new Date(b.date ?? 0).getTime();
      return filters.SortDescending ? db - da : da - db;
    });
  const categories = (Array.isArray(categoriesQuery.data) ? categoriesQuery.data : []) as any[];
  const sources = (Array.isArray(sourcesQuery.data) ? sourcesQuery.data : []) as any[];
  const types = (Array.isArray(typesQuery.data) ? typesQuery.data : []) as any[];

  const isLoading = transactionsQuery.isLoading || categoriesQuery.isLoading || sourcesQuery.isLoading || typesQuery.isLoading;
  const isFiltering = transactionsQuery.isFetching && !transactionsQuery.isLoading;

  const invalidateTransactions = () => {
    queryClient.invalidateQueries({ queryKey: ['/api/Transaction'] });
    queryClient.invalidateQueries({ queryKey: ['/api/Source'] });
  };

  const clearFilters = () => {
    setFilters({
      SortBy: 'Date',
      SortDescending: true,
    });
  };

  const handleCreateOpen = () => {
    setCreateErrors({});
    setFormData({
      amount: '',
      date: getLocalDatetime(),
      sourceId: '',
      transactionTypeId: '',
      categoryId: '',
      comment: '',
    });
    setIsCreateModalOpen(true);
  };

  // Edit UI removed — keep hooks only

  const validateTransaction = (mode: 'create' | 'edit') => {
    const errors: { amount?: string; date?: string; sourceId?: string; transactionTypeId?: string } = {};

    if (!formData.date) errors.date = 'Оберіть дату';
    if (!formData.sourceId) errors.sourceId = 'Виберіть джерело';
    if (!formData.transactionTypeId) errors.transactionTypeId = 'Виберіть тип транзакції';
    if (!formData.amount || Number(formData.amount) <= 0) errors.amount = 'Сума має бути більшою за 0';

    if (mode === 'create') setCreateErrors(errors);
    else setEditErrors(errors);

    return Object.keys(errors).length === 0;
  };

  const handleCreateSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateTransaction('create')) return;
    try {
      const source = sources.find(s => s.id === Number(formData.sourceId));
      await createMutation.mutateAsync({
        data: {
          amount: Number(formData.amount),
          date: formData.date,
          sourceId: Number(formData.sourceId),
          transactionTypeId: Number(formData.transactionTypeId),
          categoryId: formData.categoryId ? Number(formData.categoryId) : null,
          comment: formData.comment || '',
          currencyId: source?.currency?.id,
        },
      });
      setIsCreateModalOpen(false);
      setFormData({ amount: '', date: getLocalDatetime(), sourceId: '', transactionTypeId: '', categoryId: '', comment: '' });
      setCreateErrors({});
      invalidateTransactions();
    } catch (error: any) {
      console.error('Error creating transaction:', error?.response?.data || error);
      if (error?.response?.data?.errors) {
        alert(JSON.stringify(error.response.data.errors, null, 2));
      }
    }
  };

  // Update handler removed (editing disabled)

  const handleDelete = (id: number) => {
    setTransactionToDelete(id);
  };

  const confirmDelete = async () => {
    if (!transactionToDelete) return;
    try {
      await deleteMutation.mutateAsync({ id: transactionToDelete });
      invalidateTransactions();
    } catch (error) {
      console.error('Error deleting transaction:', error);
    } finally {
      setTransactionToDelete(null);
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-8">
        <div className="flex items-center justify-between">
          <Skeleton className="h-8 w-48" />
          <Skeleton className="h-11 w-44 rounded-xl" />
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 bg-[#f5f5f7] rounded-lg p-4">
          <Skeleton className="h-24 rounded-2xl" />
          <Skeleton className="h-24 rounded-2xl" />
        </div>
        <div className="space-y-3">
          <Skeleton className="h-14 rounded-2xl" />
          <Skeleton className="h-14 rounded-2xl" />
          <Skeleton className="h-14 rounded-2xl" />
          <Skeleton className="h-14 rounded-2xl" />
        </div>
      </div>
    );
  }

  const TransactionForm = ({
    errors,
    isSubmitting,
    onClose,
    submitLabel,
  }: {
    errors: { amount?: string; date?: string; sourceId?: string; transactionTypeId?: string };
    isSubmitting: boolean;
    onClose: () => void;
    submitLabel: string;
  }) => (
    <div className="space-y-4">
      <div>
        <label className="block text-sm font-semibold text-ink mb-2">Тип</label>
        <select
          value={formData.transactionTypeId}
          onChange={(e) => {
            setFormData({ ...formData, transactionTypeId: e.target.value });
            if (errors.transactionTypeId) {
              submitLabel === 'Створити'
                ? setCreateErrors((p) => ({ ...p, transactionTypeId: undefined }))
                : setEditErrors((p) => ({ ...p, transactionTypeId: undefined }));
            }
          }}
          className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]"
        >
          <option value="">Виберіть тип</option>
          {types.map((type: any) => (
            <option key={type.id} value={type.id}>{getTransactionTypeLabel(type.name).label}</option>
          ))}
        </select>
        {errors.transactionTypeId && <p className="mt-1 text-xs text-red-500">{errors.transactionTypeId}</p>}
      </div>

      <div>
        <label className="block text-sm font-semibold text-ink mb-2">Сума</label>
        <input
          type="number"
          value={formData.amount}
          onChange={(e) => {
            setFormData({ ...formData, amount: e.target.value });
            if (errors.amount) {
              submitLabel === 'Створити'
                ? setCreateErrors((p) => ({ ...p, amount: undefined }))
                : setEditErrors((p) => ({ ...p, amount: undefined }));
            }
          }}
          placeholder="0.00"
          step="0.01"
          min="0.01"
          className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]"
        />
        {errors.amount && <p className="mt-1 text-xs text-red-500">{errors.amount}</p>}
      </div>

      <div>
        <label className="block text-sm font-semibold text-ink mb-2">Дата</label>
        <input
          type="datetime-local"
          step="1"
          value={formData.date}
          onChange={(e) => {
            setFormData({ ...formData, date: e.target.value });
            if (errors.date) {
              submitLabel === 'Створити'
                ? setCreateErrors((p) => ({ ...p, date: undefined }))
                : setEditErrors((p) => ({ ...p, date: undefined }));
            }
          }}
          className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]"
        />
        {errors.date && <p className="mt-1 text-xs text-red-500">{errors.date}</p>}
      </div>

      <div>
        <label className="block text-sm font-semibold text-ink mb-2">Джерело</label>
        <select
          value={formData.sourceId}
          onChange={(e) => {
            setFormData({ ...formData, sourceId: e.target.value });
            if (errors.sourceId) {
              submitLabel === 'Створити'
                ? setCreateErrors((p) => ({ ...p, sourceId: undefined }))
                : setEditErrors((p) => ({ ...p, sourceId: undefined }));
            }
          }}
          className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]"
        >
          <option value="">Виберіть джерело</option>
          {sources.map((source: any) => (
            <option key={source.id} value={source.id}>{source.name}</option>
          ))}
        </select>
        {errors.sourceId && <p className="mt-1 text-xs text-red-500">{errors.sourceId}</p>}
      </div>

      <div>
        <label className="block text-sm font-semibold text-ink mb-2">Категорія (опціонально)</label>
        <select
          value={formData.categoryId}
          onChange={(e) => setFormData({ ...formData, categoryId: e.target.value })}
          className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]"
        >
          <option value="">Без категорії</option>
          {categories.map((category: any) => (
            <option key={category.id} value={category.id}>{category.name}</option>
          ))}
        </select>
      </div>

      <div>
        <label className="block text-sm font-semibold text-ink mb-2">Коментар (опціонально)</label>
        <textarea
          value={formData.comment}
          onChange={(e) => setFormData({ ...formData, comment: e.target.value })}
          placeholder="Додайте коментар..."
          rows={3}
          className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f] resize-none"
        />
      </div>

      <div className="flex gap-3 pt-4">
        <Button variant="secondary" onClick={onClose} type="button">
          Скасувати
        </Button>
        <Button type="submit" isLoading={isSubmitting}>
          {submitLabel}
        </Button>
      </div>
    </div>
  );

  return (
    <div className="w-full">
      <DashboardSection
        title="Транзакції"
        action={
          <div className="flex gap-2 flex-wrap justify-end">
            <Button variant="secondary" onClick={clearFilters} type="button">
              Скинути фільтри
            </Button>
            <Button onClick={handleCreateOpen}>+ Нова транзакція</Button>
          </div>
        }
      >
        {/* Filters */}
        <TransactionFilter
          filters={filters}
          onFilterChange={setFilters}
          onClearFilters={clearFilters}
        />

        {/* Table with loading overlay */}
        <div className="relative">
          {isFiltering && (
            <div className="absolute inset-0 bg-white/60 z-10 flex items-center justify-center rounded-xl">
              <div className="flex items-center gap-2 text-sm text-primary font-medium bg-white px-4 py-2 rounded-full shadow-sm border border-primary/20">
                <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24" fill="none">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="3" />
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v4l3-3-3-3v4a8 8 0 00-8 8h4z" />
                </svg>
                Фільтрується...
              </div>
            </div>
          )}
          {transactions.length > 0 ? (
            <TransactionTable
              transactions={transactions as any}
              onDelete={handleDelete}
            />
          ) : (
            <EmptyState
              title="Немає транзакцій"
              description="Поки що тут порожньо. Додайте першу транзакцію, щоб отримати аналітику по доходах і витратах."
              action={<Button onClick={handleCreateOpen}>+ Нова транзакція</Button>}
            />
          )}
        </div>

        {transactions.length > 0 && (
          <p className="text-xs text-[#7a7a7a] mt-4">
            Всього: {transactions.length} транзакцій
          </p>
        )}
      </DashboardSection>

      {/* Create Modal */}
      <Modal
        isOpen={isCreateModalOpen}
        title="Нова транзакція"
        onClose={() => setIsCreateModalOpen(false)}
        size="md"
      >
        <form onSubmit={handleCreateSubmit}>
          {TransactionForm({
            errors: createErrors,
            isSubmitting: createMutation.isPending,
            onClose: () => setIsCreateModalOpen(false),
            submitLabel: "Створити",
          })}
        </form>
      </Modal>

      {/* Edit Modal */}
      {/* Edit UI removed */}

      <ConfirmModal
        isOpen={transactionToDelete !== null}
        title="Видалення транзакції"
        message="Ви впевнені, що хочете видалити цю транзакцію? Цю дію неможливо скасувати."
        onConfirm={confirmDelete}
        onCancel={() => setTransactionToDelete(null)}
      />
    </div>
  );
};
