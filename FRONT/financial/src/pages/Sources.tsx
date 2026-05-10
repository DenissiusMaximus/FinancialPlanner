import React, { useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { DashboardSection } from '../components/DashboardSection';
import { SourceCard } from '../components/SourceCard';
import { Button } from '../components/Button';
import { Modal } from '../components/Modal';
import { Skeleton } from '../components/Skeleton';
import { EmptyState } from '../components/EmptyState';
import {
  useGetApiSource,
  usePostApiSource,
  usePatchApiSourceId,
  useDeleteApiSourceId,
  useGetApiCurrency,
} from '../api/generated/endpoints';

export const Sources: React.FC = () => {
  const queryClient = useQueryClient();
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [editingSource, setEditingSource] = useState<any>(null);
  const [formData, setFormData] = useState<{ name: string; currencyId: string | number; amount: number }>({
    name: '',
    currencyId: '',
    amount: 0,
  });
  const [createErrors, setCreateErrors] = useState<{ name?: string; currencyId?: string; amount?: string }>({});
  const [editErrors, setEditErrors] = useState<{ name?: string }>({});

  // API Queries
  const sourcesQuery = useGetApiSource();
  const currenciesQuery = useGetApiCurrency();

  // Mutations
  const createMutation = usePostApiSource();
  const updateMutation = usePatchApiSourceId();
  const deleteMutation = useDeleteApiSourceId();

  const sources = (Array.isArray(sourcesQuery.data) ? sourcesQuery.data : []) as any[];
  const currencies = (Array.isArray(currenciesQuery.data) ? currenciesQuery.data : []) as any[];
  const isLoading = sourcesQuery.isLoading || currenciesQuery.isLoading;

  const invalidateSources = () => {
    queryClient.invalidateQueries({ queryKey: ['/api/Source'] });
  };

  const handleCreateOpen = () => {
    setFormData({ name: '', currencyId: '', amount: 0 });
    setCreateErrors({});
    setIsCreateModalOpen(true);
  };

  const handleEditOpen = (source: any) => {
    setEditingSource(source);
    setFormData({
      name: source.name,
      currencyId: source.currency?.id ?? '',
      amount: source.amount,
    });
    setEditErrors({});
    setIsEditModalOpen(true);
  };

  const validateCreate = () => {
    const nextErrors: { name?: string; currencyId?: string; amount?: string } = {};

    if (!String(formData.name || '').trim()) nextErrors.name = 'Вкажіть назву';
    if (!formData.currencyId) nextErrors.currencyId = 'Виберіть валюту';
    if (formData.amount === null || formData.amount === undefined || isNaN(Number(formData.amount))) {
      nextErrors.amount = 'Вкажіть суму';
    }

    setCreateErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  };

  const validateEdit = () => {
    const nextErrors: { name?: string } = {};
    if (!String(formData.name || '').trim()) nextErrors.name = 'Вкажіть назву';
    setEditErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  };

  const handleCreateSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateCreate()) return;

    try {
      await createMutation.mutateAsync({
        data: {
          name: formData.name,
          currencyId: Number(formData.currencyId),
          amount: Number(formData.amount),
        },
      });
      setIsCreateModalOpen(false);
      setFormData({ name: '', currencyId: '', amount: 0 });
      setCreateErrors({});
      invalidateSources();
    } catch (error) {
      console.error('Error creating source:', error);
    }
  };

  const handleUpdateSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingSource?.id) return;
    if (!validateEdit()) return;

    try {
      await updateMutation.mutateAsync({
        id: editingSource.id,
        data: { name: formData.name },
      });
      setIsEditModalOpen(false);
      setEditingSource(null);
      setEditErrors({});
      invalidateSources();
    } catch (error) {
      console.error('Error updating source:', error);
    }
  };

  const handleDelete = async (id: number) => {
    if (confirm('Ви впевнені, що хочете видалити це джерело?')) {
      try {
        await deleteMutation.mutateAsync({ id });
        invalidateSources();
      } catch (error) {
        console.error('Error deleting source:', error);
      }
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-8">
        <div className="flex items-center justify-between">
          <Skeleton className="h-8 w-56" />
          <Skeleton className="h-11 w-36 rounded-xl" />
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
          <Skeleton className="h-40 rounded-2xl" />
          <Skeleton className="h-40 rounded-2xl" />
          <Skeleton className="h-40 rounded-2xl" />
          <Skeleton className="h-40 rounded-2xl" />
        </div>
      </div>
    );
  }

  return (
    <div className="w-full">
      <DashboardSection
        title="Мої джерела"
        action={<Button onClick={handleCreateOpen}>+ Нове джерело</Button>}
      >
        {sources.length > 0 ? (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
            {sources.map((source) => (
              <SourceCard
                key={source.id}
                source={source as any}
                onEdit={handleEditOpen}
                onDelete={handleDelete}
              />
            ))}
          </div>
        ) : (
          <EmptyState
            title="Немає джерел"
            description="Створіть перше джерело, щоб почати керувати своїми коштами, валютою та балансом."
            action={<Button onClick={handleCreateOpen}>+ Нове джерело</Button>}
          />
        )}
      </DashboardSection>

      {/* Create Modal */}
      <Modal
        isOpen={isCreateModalOpen}
        title="Нове джерело"
        onClose={() => setIsCreateModalOpen(false)}
      >
        <form onSubmit={handleCreateSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-semibold text-ink mb-2">Назва</label>
            <input
              type="text"
              value={formData.name || ''}
              onChange={(e) => {
                setFormData({ ...formData, name: e.target.value });
                if (createErrors.name) setCreateErrors((prev) => ({ ...prev, name: undefined }));
              }}
              placeholder="Наприклад: Основний рахунок"
              className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]"
              required
            />
            {createErrors.name && <p className="mt-1 text-xs text-red-500">{createErrors.name}</p>}
          </div>

          <div>
            <label className="block text-sm font-semibold text-ink mb-2">Валюта</label>
            <select
              value={formData.currencyId}
              onChange={(e) => {
                setFormData({ ...formData, currencyId: e.target.value });
                if (createErrors.currencyId) setCreateErrors((prev) => ({ ...prev, currencyId: undefined }));
              }}
              className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]"
              required
            >
              <option value="">Виберіть валюту</option>
              {currencies.map((currency: any) => (
                <option key={currency.id} value={currency.id}>
                  {currency.name}
                </option>
              ))}
            </select>
            {createErrors.currencyId && <p className="mt-1 text-xs text-red-500">{createErrors.currencyId}</p>}
          </div>

          <div>
            <label className="block text-sm font-semibold text-ink mb-2">Початкова сума</label>
            <input
              type="number"
              value={formData.amount}
              onChange={(e) => {
                setFormData({ ...formData, amount: Number(e.target.value) });
                if (createErrors.amount) setCreateErrors((prev) => ({ ...prev, amount: undefined }));
              }}
              placeholder="0.00"
              step="0.01"
              min="0"
              className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]"
            />
            {createErrors.amount && <p className="mt-1 text-xs text-red-500">{createErrors.amount}</p>}
          </div>

          <div className="flex gap-3 pt-4">
            <Button variant="secondary" onClick={() => setIsCreateModalOpen(false)} type="button">
              Скасувати
            </Button>
            <Button type="submit" isLoading={createMutation.isPending}>
              Створити
            </Button>
          </div>
        </form>
      </Modal>

      {/* Edit Modal */}
      <Modal
        isOpen={isEditModalOpen}
        title="Редагувати джерело"
        onClose={() => setIsEditModalOpen(false)}
      >
        <form onSubmit={handleUpdateSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-semibold text-ink mb-2">Назва</label>
            <input
              type="text"
              value={formData.name || ''}
              onChange={(e) => {
                setFormData({ ...formData, name: e.target.value });
                if (editErrors.name) setEditErrors((prev) => ({ ...prev, name: undefined }));
              }}
              placeholder="Наприклад: Основний рахунок"
              className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]"
              required
            />
            {editErrors.name && <p className="mt-1 text-xs text-red-500">{editErrors.name}</p>}
          </div>

          <div className="flex gap-3 pt-4">
            <Button variant="secondary" onClick={() => setIsEditModalOpen(false)} type="button">
              Скасувати
            </Button>
            <Button type="submit" isLoading={updateMutation.isPending}>
              Зберегти
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
};
