import React, { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { DashboardSection } from '../components/DashboardSection';
import { Button } from '../components/Button';
import { Modal } from '../components/Modal';
import { Skeleton } from '../components/Skeleton';
import { EmptyState } from '../components/EmptyState';
import { customInstance } from '../api/custom-instance';
import { useGetApiSource, useGetApiCategory, useGetApiTransactionType, useGetApiCurrency } from '../api/generated/endpoints';
import type {
  PlannedTransactionDto,
  CreatePlannedTransactionInput,
  FrequencyDto,
} from '../types/generated';
import { getTransactionTypeLabel } from '../utils/display-helpers';

const PLANNED_TX_KEY = ['/api/PlannedTransaction'];
const FREQUENCY_KEY = ['/api/Frequency'];

const getPlannedTransactions = () =>
  customInstance<PlannedTransactionDto[]>({ url: '/api/PlannedTransaction', method: 'GET' });

const getFrequencies = () =>
  customInstance<FrequencyDto[]>({ url: '/api/Frequency', method: 'GET' });

const createPlannedTransaction = (data: CreatePlannedTransactionInput) =>
  customInstance<PlannedTransactionDto>({
    url: '/api/PlannedTransaction',
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    data,
  });

const updatePlannedTransaction = (id: number, data: Partial<CreatePlannedTransactionInput>) =>
  customInstance<PlannedTransactionDto>({
    url: `/api/PlannedTransaction/${id}`,
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json' },
    data,
  });

const deletePlannedTransaction = (id: number) =>
  customInstance<void>({ url: `/api/PlannedTransaction/${id}`, method: 'DELETE' });

const IconEdit = () => (
  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
    <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
  </svg>
);

const IconTrash = () => (
  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <polyline points="3 6 5 6 21 6" /><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6" />
    <path d="M10 11v6" /><path d="M14 11v6" /><path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2" />
  </svg>
);

type FormState = {
  name: string;
  amount: string;
  startDate: string;
  sourceId: string;
  transactionTypeId: string;
  frequencyId: string;
  categoryId: string;
  currencyId: string;
};

const emptyForm = (): FormState => ({
  name: '',
  amount: '',
  startDate: new Date().toISOString().split('T')[0],
  sourceId: '',
  transactionTypeId: '',
  frequencyId: '',
  categoryId: '',
  currencyId: '',
});

export const PlannedTransactions: React.FC = () => {
  const queryClient = useQueryClient();
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<PlannedTransactionDto | null>(null);
  const [formData, setFormData] = useState<FormState>(emptyForm());
  const [createErrors, setCreateErrors] = useState<Partial<Record<keyof FormState, string>>>({});
  const [editErrors, setEditErrors] = useState<Partial<Record<keyof FormState, string>>>({});

  // Queries
  const plannedQuery = useQuery({ queryKey: PLANNED_TX_KEY, queryFn: getPlannedTransactions });
  const frequencyQuery = useQuery({ queryKey: FREQUENCY_KEY, queryFn: getFrequencies });
  const sourcesQuery = useGetApiSource();
  const categoriesQuery = useGetApiCategory();
  const typesQuery = useGetApiTransactionType();
  const currenciesQuery = useGetApiCurrency();

  // Mutations
  const createMutation = useMutation({
    mutationFn: (data: CreatePlannedTransactionInput) => createPlannedTransaction(data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: PLANNED_TX_KEY }),
  });
  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Partial<CreatePlannedTransactionInput> }) =>
      updatePlannedTransaction(id, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: PLANNED_TX_KEY }),
  });
  const deleteMutation = useMutation({
    mutationFn: (id: number) => deletePlannedTransaction(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: PLANNED_TX_KEY }),
  });

  const plannedRaw = plannedQuery.data as any;
  const planned = (Array.isArray(plannedRaw?.data) ? plannedRaw.data : Array.isArray(plannedRaw) ? plannedRaw : []) as PlannedTransactionDto[];
  
  const frequenciesRaw = frequencyQuery.data as any;
  const frequencies = (Array.isArray(frequenciesRaw?.data) ? frequenciesRaw.data : Array.isArray(frequenciesRaw) ? frequenciesRaw : []) as FrequencyDto[];
  
  const sourcesRaw = sourcesQuery.data as any;
  const sources = (Array.isArray(sourcesRaw?.data) ? sourcesRaw.data : Array.isArray(sourcesRaw) ? sourcesRaw : []) as any[];
  
  const categoriesRaw = categoriesQuery.data as any;
  const categories = (Array.isArray(categoriesRaw?.data) ? categoriesRaw.data : Array.isArray(categoriesRaw) ? categoriesRaw : []) as any[];
  
  const typesRaw = typesQuery.data as any;
  const types = (Array.isArray(typesRaw?.data) ? typesRaw.data : Array.isArray(typesRaw) ? typesRaw : []) as any[];
  
  const currenciesRaw = currenciesQuery.data as any;
  const currencies = (Array.isArray(currenciesRaw?.data) ? currenciesRaw.data : Array.isArray(currenciesRaw) ? currenciesRaw : []) as any[];

  const isLoading = plannedQuery.isLoading || frequencyQuery.isLoading || sourcesQuery.isLoading || currenciesQuery.isLoading;

  const validate = (mode: 'create' | 'edit') => {
    const errors: Partial<Record<keyof FormState, string>> = {};
    if (!formData.amount || Number(formData.amount) <= 0) errors.amount = 'Сума має бути більшою за 0';
    if (!formData.startDate) errors.startDate = 'Оберіть дату початку';
    if (!formData.sourceId) errors.sourceId = 'Виберіть джерело';
    if (!formData.transactionTypeId) errors.transactionTypeId = 'Виберіть тип';
    if (!formData.frequencyId) errors.frequencyId = 'Виберіть частоту';
    if (!formData.currencyId) errors.currencyId = 'Виберіть валюту';

    if (mode === 'create') setCreateErrors(errors);
    else setEditErrors(errors);

    return Object.keys(errors).length === 0;
  };

  const handleCreateSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate('create')) return;
    await createMutation.mutateAsync({
      name: formData.name || null,
      amount: Number(formData.amount),
      startDate: formData.startDate,
      sourceId: Number(formData.sourceId),
      transactionTypeId: Number(formData.transactionTypeId),
      frequencyId: Number(formData.frequencyId),
      categoryId: formData.categoryId ? Number(formData.categoryId) : null,
      currencyId: Number(formData.currencyId),
    });
    setIsCreateOpen(false);
    setFormData(emptyForm());
    setCreateErrors({});
  };

  const handleEditSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingItem?.id) return;
    if (!validate('edit')) return;
    await updateMutation.mutateAsync({
      id: editingItem.id,
      data: {
        name: formData.name || null,
        amount: Number(formData.amount),
        startDate: formData.startDate,
        sourceId: Number(formData.sourceId),
        transactionTypeId: Number(formData.transactionTypeId),
        frequencyId: Number(formData.frequencyId),
        categoryId: formData.categoryId ? Number(formData.categoryId) : null,
        currencyId: Number(formData.currencyId),
      },
    });
    setIsEditOpen(false);
    setEditingItem(null);
    setEditErrors({});
  };

  const handleDelete = async (id: number) => {
    if (confirm('Видалити заплановану транзакцію?')) {
      await deleteMutation.mutateAsync(id);
    }
  };

  const handleEditOpen = (item: PlannedTransactionDto) => {
    setEditingItem(item);
    setFormData({
      name: item.name ?? '',
      amount: String(item.amount ?? ''),
      startDate: item.startDate?.split('T')[0] ?? new Date().toISOString().split('T')[0],
      sourceId: String(item.source?.id ?? ''),
      transactionTypeId: String(item.transactionType?.id ?? ''),
      frequencyId: String(item.frequency?.id ?? ''),
      categoryId: String(item.category?.id ?? ''),
      currencyId: String(item.currency?.id ?? ''),
    });
    setEditErrors({});
    setIsEditOpen(true);
  };

  const getTypeColor = (name: string) => {
    const n = (name || '').toLowerCase();
    if (n === 'income' || n === 'дохід') return '#34c759';
    if (n === 'expense' || n === 'витрата') return '#ff3b30';
    return '#0066cc';
  };

  if (isLoading) {
    return (
      <div className="space-y-8">
        <div className="flex items-center justify-between">
          <Skeleton className="h-8 w-56" />
          <Skeleton className="h-11 w-44 rounded-xl" />
        </div>
        <div className="space-y-3">
          <Skeleton className="h-16 rounded-xl" />
          <Skeleton className="h-16 rounded-xl" />
          <Skeleton className="h-16 rounded-xl" />
        </div>
      </div>
    );
  }

  const PlannedForm = ({
    errors,
    isSubmitting,
    onClose,
    submitLabel,
  }: {
    errors: Partial<Record<keyof FormState, string>>;
    isSubmitting: boolean;
    onClose: () => void;
    submitLabel: string;
  }) => (
    <div className="space-y-4">
      <div>
        <label className="block text-sm font-semibold text-ink mb-2">Назва (опціонально)</label>
        <input
          type="text"
          value={formData.name}
          onChange={(e) => setFormData({ ...formData, name: e.target.value })}
          placeholder="Наприклад: Зарплата"
          className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]"
        />
      </div>

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
          {types.map((t: any) => <option key={t.id} value={t.id}>{getTransactionTypeLabel(t.name).label}</option>)}
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
        <label className="block text-sm font-semibold text-ink mb-2">Валюта</label>
        <select
          value={formData.currencyId}
          onChange={(e) => {
            setFormData({ ...formData, currencyId: e.target.value });
            if (errors.currencyId) {
              submitLabel === 'Створити'
                ? setCreateErrors((p) => ({ ...p, currencyId: undefined }))
                : setEditErrors((p) => ({ ...p, currencyId: undefined }));
            }
          }}
          className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]"
        >
          <option value="">Виберіть валюту</option>
          {currencies.map((c: any) => <option key={c.id} value={c.id}>{c.name}</option>)}
        </select>
        {errors.currencyId && <p className="mt-1 text-xs text-red-500">{errors.currencyId}</p>}
      </div>

      <div>
        <label className="block text-sm font-semibold text-ink mb-2">Дата початку</label>
        <input
          type="date"
          value={formData.startDate}
          onChange={(e) => {
            setFormData({ ...formData, startDate: e.target.value });
            if (errors.startDate) {
              submitLabel === 'Створити'
                ? setCreateErrors((p) => ({ ...p, startDate: undefined }))
                : setEditErrors((p) => ({ ...p, startDate: undefined }));
            }
          }}
          className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]"
        />
        {errors.startDate && <p className="mt-1 text-xs text-red-500">{errors.startDate}</p>}
      </div>

      <div>
        <label className="block text-sm font-semibold text-ink mb-2">Частота</label>
        <select
          value={formData.frequencyId}
          onChange={(e) => {
            setFormData({ ...formData, frequencyId: e.target.value });
            if (errors.frequencyId) {
              submitLabel === 'Створити'
                ? setCreateErrors((p) => ({ ...p, frequencyId: undefined }))
                : setEditErrors((p) => ({ ...p, frequencyId: undefined }));
            }
          }}
          className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]"
        >
          <option value="">Виберіть частоту</option>
          {frequencies.map((f) => <option key={f.id} value={f.id!}>{f.name}</option>)}
        </select>
        {errors.frequencyId && <p className="mt-1 text-xs text-red-500">{errors.frequencyId}</p>}
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
          {sources.map((s: any) => <option key={s.id} value={s.id}>{s.name}</option>)}
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
          {categories.map((c: any) => <option key={c.id} value={c.id}>{c.name}</option>)}
        </select>
      </div>

      <div className="flex gap-3 pt-4">
        <Button variant="secondary" onClick={onClose} type="button">Скасувати</Button>
        <Button type="submit" isLoading={isSubmitting}>{submitLabel}</Button>
      </div>
    </div>
  );

  return (
    <div className="w-full">
      <DashboardSection
        title="Планові транзакції"
        action={<Button onClick={() => { setFormData(emptyForm()); setCreateErrors({}); setIsCreateOpen(true); }}>+ Нова планова транзакція</Button>}
      >
        {planned.length > 0 ? (
          <div className="w-full">
            {/* Mobile card list */}
            <div className="sm:hidden space-y-2">
              {planned.map((item) => (
                <div
                  key={item.id}
                  className="bg-white border border-[#f0f0f0] rounded-xl p-4"
                >
                  <div className="flex items-start justify-between gap-3">
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-2 flex-wrap mb-1">
                        {item.transactionType && (
                          <span
                            className="text-xs font-semibold px-2 py-0.5 rounded-full"
                            style={{
                              color: getTypeColor(item.transactionType.name ?? ''),
                              backgroundColor: getTypeColor(item.transactionType.name ?? '') + '18',
                            }}
                          >
                            {item.transactionType.name}
                          </span>
                        )}
                        {item.frequency?.name && (
                          <span className="text-xs text-[#7a7a7a] bg-[#f5f5f7] px-2 py-0.5 rounded-full">
                            {item.frequency.name}
                          </span>
                        )}
                      </div>
                      <div className="font-semibold text-ink text-sm truncate">
                        {item.name || item.source?.name || '—'}
                      </div>
                      <div className="flex items-center gap-3 mt-1 flex-wrap">
                        <span
                          className="font-mono font-bold text-base"
                          style={{ color: getTypeColor(item.transactionType?.name ?? '') }}
                        >
                          {(item.amount ?? 0).toFixed(2)}
                          <span className="text-xs font-normal ml-1">{item.currency?.name}</span>
                        </span>
                        {item.source?.name && (
                          <span className="text-xs text-[#7a7a7a]">{item.source.name}</span>
                        )}
                      </div>
                    </div>
                    <div className="flex gap-1 shrink-0">
                      <button
                        className="text-[#7a7a7a] hover:text-primary transition-colors p-2 rounded-lg hover:bg-primary/5 touch-manipulation"
                        onClick={() => handleEditOpen(item)}
                        title="Редагувати"
                      >
                        <IconEdit />
                      </button>
                      <button
                        className="text-[#7a7a7a] hover:text-red-500 transition-colors p-2 rounded-lg hover:bg-red-50 touch-manipulation"
                        onClick={() => item.id && handleDelete(item.id)}
                        title="Видалити"
                      >
                        <IconTrash />
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>

            {/* Desktop table */}
            <div className="hidden sm:block overflow-x-auto">
              <table className="w-full border-collapse text-sm">
                <thead>
                  <tr className="bg-[#f5f5f7] border-b border-hairline">
                    <th className="px-4 py-3 text-left font-semibold text-ink text-xs uppercase tracking-wider">Назва</th>
                    <th className="px-4 py-3 text-left font-semibold text-ink text-xs uppercase tracking-wider">Тип</th>
                    <th className="px-4 py-3 text-left font-semibold text-ink text-xs uppercase tracking-wider">Сума</th>
                    <th className="px-4 py-3 text-left font-semibold text-ink text-xs uppercase tracking-wider">Дата початку</th>
                    <th className="px-4 py-3 text-left font-semibold text-ink text-xs uppercase tracking-wider">Частота</th>
                    <th className="px-4 py-3 text-left font-semibold text-ink text-xs uppercase tracking-wider">Джерело</th>
                    <th className="px-4 py-3 text-left font-semibold text-ink text-xs uppercase tracking-wider">Категорія</th>
                    <th className="px-4 py-3 text-left font-semibold text-ink text-xs uppercase tracking-wider">Дії</th>
                  </tr>
                </thead>
                <tbody>
                  {planned.map((item) => (
                    <tr key={item.id} className="border-b border-[#f0f0f0] hover:bg-[#fafafc] transition-colors">
                      <td className="px-4 py-3 text-ink font-medium">{item.name || '—'}</td>
                      <td className="px-4 py-3">
                        {item.transactionType && (
                          <span
                            className="font-medium text-xs px-2 py-0.5 rounded-full"
                            style={{
                              color: getTypeColor(item.transactionType.name ?? ''),
                              backgroundColor: getTypeColor(item.transactionType.name ?? '') + '18',
                            }}
                          >
                            {item.transactionType.name}
                          </span>
                        )}
                      </td>
                      <td
                        className="px-4 py-3 font-mono font-semibold"
                        style={{ color: getTypeColor(item.transactionType?.name ?? '') }}
                      >
                        {(item.amount ?? 0).toFixed(2)} {item.currency?.name ?? ''}
                      </td>
                      <td className="px-4 py-3 text-[#7a7a7a] font-mono text-xs">
                        {item.startDate ? new Date(item.startDate).toLocaleDateString('uk-UA') : '—'}
                      </td>
                      <td className="px-4 py-3 text-ink">{item.frequency?.name ?? '—'}</td>
                      <td className="px-4 py-3 text-ink">{item.source?.name ?? '—'}</td>
                      <td className="px-4 py-3 text-ink">{item.category?.name ?? '—'}</td>
                      <td className="px-4 py-3">
                        <div className="flex gap-1">
                          <button
                            className="text-[#7a7a7a] hover:text-primary transition-colors p-1.5 rounded-lg hover:bg-primary/5"
                            onClick={() => handleEditOpen(item)}
                            title="Редагувати"
                          >
                            <IconEdit />
                          </button>
                          <button
                            className="text-[#7a7a7a] hover:text-red-500 transition-colors p-1.5 rounded-lg hover:bg-red-50"
                            onClick={() => item.id && handleDelete(item.id)}
                            title="Видалити"
                          >
                            <IconTrash />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        ) : (
          <EmptyState
            title="Немає планових транзакцій"
            description="Додайте заплановану транзакцію (наприклад, регулярний дохід чи витрату)."
            action={
              <Button onClick={() => { setFormData(emptyForm()); setCreateErrors({}); setIsCreateOpen(true); }}>
                + Нова планова транзакція
              </Button>
            }
          />
        )}
      </DashboardSection>

      {/* Create Modal */}
      <Modal isOpen={isCreateOpen} title="Нова планова транзакція" onClose={() => setIsCreateOpen(false)} size="md">
        <form onSubmit={handleCreateSubmit}>
          {PlannedForm({
            errors: createErrors,
            isSubmitting: createMutation.isPending,
            onClose: () => setIsCreateOpen(false),
            submitLabel: "Створити",
          })}
        </form>
      </Modal>

      {/* Edit Modal */}
      <Modal isOpen={isEditOpen} title="Редагувати планову транзакцію" onClose={() => setIsEditOpen(false)} size="md">
        <form onSubmit={handleEditSubmit}>
          {PlannedForm({
            errors: editErrors,
            isSubmitting: updateMutation.isPending,
            onClose: () => setIsEditOpen(false),
            submitLabel: "Зберегти",
          })}
        </form>
      </Modal>
    </div>
  );
};
