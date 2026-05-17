import React, { useEffect, useMemo, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import axios from 'axios';
import { useQueryClient } from '@tanstack/react-query';
import {
  DndContext,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  TouchSensor,
  useSensor,
  useSensors,
  type DragEndEvent,
} from '@dnd-kit/core';
import { SortableContext, sortableKeyboardCoordinates, verticalListSortingStrategy } from '@dnd-kit/sortable';
import { AimProgressCard } from '../components/AimProgressCard';
import { Button } from '../components/Button';
import { Modal } from '../components/Modal';
import { SourceSelectionModal } from '../components/SourceSelectionModal';
import { Skeleton } from '../components/Skeleton';
import { EmptyState } from '../components/EmptyState';
import {
  useGetApiAim,
  usePostApiAim,
  usePatchApiAimId,
  useDeleteApiAimId,
  useGetApiCurrency,
  useGetApiSource,
  usePostApiAimAimIdSourcesSourceId,
  useDeleteApiAimAimIdSourcesSourceId,
} from '../api/generated/endpoints';

type AimFormState = {
  name: string;
  amount: string;
  priority: number;
  currencyId: string;
};

type AimErrorState = Partial<Record<'name' | 'amount' | 'priority' | 'currencyId', string>>;

const EMPTY_FORM: AimFormState = { name: '', amount: '', priority: 1, currencyId: '' };

const getErrorMessage = (error: unknown): string => {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as any;
    const messages = [
      data?.message, data?.title, data?.detail, data?.error,
      ...(data?.errors && typeof data.errors === 'object' ? Object.values(data.errors).flat() : []),
      ...(data?.domainErrors && typeof data.domainErrors === 'object' ? Object.values(data.domainErrors).flat() : []),
      ...(data?.DomainErrors && typeof data.DomainErrors === 'object' ? Object.values(data.DomainErrors).flat() : []),
    ].filter((v): v is string => typeof v === 'string' && v.trim().length > 0);
    return messages[0] ?? 'Не вдалося виконати операцію';
  }
  if (error instanceof Error) return error.message;
  return 'Не вдалося виконати операцію';
};

// ─── AimForm is defined OUTSIDE Aims so React never re-mounts the DOM nodes ───
type AimFormProps = {
  formData: AimFormState;
  onFormChange: (f: AimFormState) => void;
  errors: AimErrorState;
  onErrorChange: (e: AimErrorState) => void;
  currencies: any[];
  submitLabel: string;
  isSubmitting: boolean;
  onClose: () => void;
};

const AimForm: React.FC<AimFormProps> = ({
  formData, onFormChange, errors, onErrorChange, currencies, submitLabel, isSubmitting, onClose,
}) => (
  <div className="space-y-4">
    <div>
      <label className="block text-sm font-semibold text-ink mb-2">Назва</label>
      <input
        type="text"
        value={formData.name}
        onChange={(e) => {
          onFormChange({ ...formData, name: e.target.value });
          if (errors.name) onErrorChange({ ...errors, name: undefined });
        }}
        className="w-full rounded-lg border border-hairline px-4 py-2 text-[#1d1d1f] focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary/20"
      />
      {errors.name && <p className="mt-1 text-xs text-red-500">{errors.name}</p>}
    </div>

    <div>
      <label className="block text-sm font-semibold text-ink mb-2">Цільова сума</label>
      <input
        type="number" value={formData.amount} step="0.01" min="0.01"
        onChange={(e) => {
          onFormChange({ ...formData, amount: e.target.value });
          if (errors.amount) onErrorChange({ ...errors, amount: undefined });
        }}
        className="w-full rounded-lg border border-hairline px-4 py-2 text-[#1d1d1f] focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary/20"
      />
      {errors.amount && <p className="mt-1 text-xs text-red-500">{errors.amount}</p>}
    </div>

    <div>
      <label className="block text-sm font-semibold text-ink mb-2">Пріоритет</label>
      <input
        type="number" value={formData.priority} min="1"
        onChange={(e) => {
          onFormChange({ ...formData, priority: Number(e.target.value) });
          if (errors.priority) onErrorChange({ ...errors, priority: undefined });
        }}
        className="w-full rounded-lg border border-hairline px-4 py-2 text-[#1d1d1f] focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary/20"
      />
      {errors.priority && <p className="mt-1 text-xs text-red-500">{errors.priority}</p>}
    </div>

    <div>
      <label className="block text-sm font-semibold text-ink mb-2">Валюта</label>
      <select
        value={formData.currencyId}
        onChange={(e) => {
          onFormChange({ ...formData, currencyId: e.target.value });
          if (errors.currencyId) onErrorChange({ ...errors, currencyId: undefined });
        }}
        className="w-full rounded-lg border border-hairline px-4 py-2 text-[#1d1d1f] focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary/20"
      >
        <option value="">Оберіть валюту</option>
        {currencies.map((c: any) => (
          <option key={c.id} value={c.id}>{c.name}</option>
        ))}
      </select>
      {errors.currencyId && <p className="mt-1 text-xs text-red-500">{errors.currencyId}</p>}
    </div>

    <div className="flex gap-3 pt-4">
      <Button variant="secondary" onClick={onClose} type="button">Скасувати</Button>
      <Button type="submit" isLoading={isSubmitting}>{submitLabel}</Button>
    </div>
  </div>
);
// ──────────────────────────────────────────────────────────────────────────────

export const Aims: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const queryClient = useQueryClient();
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isSourceSelectionModalOpen, setIsSourceSelectionModalOpen] = useState(false);
  const [isEditingView, setIsEditingView] = useState(false);
  const [selectedAimId, setSelectedAimId] = useState<number | null>(null);
  const [formData, setFormData] = useState<AimFormState>(EMPTY_FORM);
  const [createErrors, setCreateErrors] = useState<AimErrorState>({});
  const [editErrors, setEditErrors] = useState<AimErrorState>({});
  const [pageError, setPageError] = useState<string | null>(null);
  const [detailError, setDetailError] = useState<string | null>(null);
  const [editTriggered, setEditTriggered] = useState(false);

  // DnD Sensors — separate pointer (mouse/trackpad) from touch to avoid conflicts
  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: { distance: 5 },
    }),
    useSensor(TouchSensor, {
      activationConstraint: { delay: 200, tolerance: 8 },
    }),
    useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates }),
  );

  const aimsQuery = useGetApiAim(undefined, { query: { refetchOnWindowFocus: false } });
  const currenciesQuery = useGetApiCurrency();
  const sourcesQuery = useGetApiSource({ query: { refetchOnWindowFocus: false } });

  const createMutation = usePostApiAim();
  const updateMutation = usePatchApiAimId();
  const deleteMutation = useDeleteApiAimId();
  const addSourceMutation = usePostApiAimAimIdSourcesSourceId();
  const removeSourceMutation = useDeleteApiAimAimIdSourcesSourceId();

  // Handle both paginated { data: [] } and plain [] response formats
  const aims = (
    Array.isArray((aimsQuery.data as any)?.data)
      ? (aimsQuery.data as any).data
      : Array.isArray(aimsQuery.data) ? aimsQuery.data : []
  ) as any[];

  const currencies = (Array.isArray(currenciesQuery.data) ? currenciesQuery.data : []) as any[];
  const sources = (Array.isArray(sourcesQuery.data) ? sourcesQuery.data : []) as any[];
  const isLoading = aimsQuery.isLoading || currenciesQuery.isLoading || sourcesQuery.isLoading;

  const sortedAims = useMemo(
    () => [...aims].sort((a, b) => (a.priority ?? 0) - (b.priority ?? 0)),
    [aims],
  );

  const selectedAim = useMemo(
    () => sortedAims.find((a) => a.id === selectedAimId) ?? sortedAims[0] ?? null,
    [selectedAimId, sortedAims],
  );

  const activeSources = useMemo(() => sources.filter((s) => !s?.isArchived), [sources]);

  const linkedSourceIds = useMemo(
    () => new Set(((selectedAim?.sources ?? []) as any[]).map((s) => s?.id).filter(Boolean)),
    [selectedAim],
  );

  const availableSources = useMemo(
    () => activeSources.filter((s) => !linkedSourceIds.has(s.id)),
    [activeSources, linkedSourceIds],
  );

  // Sync selected aim when list changes
  useEffect(() => {
    if (!sortedAims.length) { setSelectedAimId(null); return; }
    const exists = selectedAimId !== null && sortedAims.some((a) => a.id === selectedAimId);
    if (!exists) setSelectedAimId(sortedAims[0]?.id ?? null);
  }, [selectedAimId, sortedAims]);

  useEffect(() => {
    setDetailError(null);
  }, [selectedAim?.id]);

  // Open edit modal when navigated here with ?editId=<id> (e.g. from Dashboard)
  useEffect(() => {
    const editId = searchParams.get('editId');
    if (editId && sortedAims.length > 0 && !editTriggered) {
      const aim = sortedAims.find((a) => a.id === Number(editId));
      if (aim) {
        setEditTriggered(true);
        handleEditOpen(aim);
        setSearchParams({}, { replace: true });
      }
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchParams, sortedAims, editTriggered]);

  const invalidateAims = () => {
    queryClient.invalidateQueries({ queryKey: ['/api/Aim'] });
    queryClient.invalidateQueries({ queryKey: ['/api/Source'] });
  };

  const validateAim = (mode: 'create' | 'edit') => {
    const e: AimErrorState = {};
    if (!String(formData.name || '').trim()) e.name = 'Вкажіть назву';
    if (Number(formData.amount) <= 0) e.amount = 'Сума має бути більшою за 0';
    if (Number(formData.priority) < 1) e.priority = 'Пріоритет має бути від 1';
    if (!formData.currencyId) e.currencyId = 'Виберіть валюту';
    if (mode === 'create') setCreateErrors(e);
    else setEditErrors(e);
    return Object.keys(e).length === 0;
  };

  const handleCreateOpen = () => {
    setFormData(EMPTY_FORM);
    setCreateErrors({});
    setPageError(null);
    setIsCreateModalOpen(true);
  };

  const isFormChanged = (aim: any): boolean => {
    if (!aim) return false;
    const currentForm = {
      name: aim.name ?? '',
      amount: aim.amount != null ? String(aim.amount) : '',
      priority: aim.priority ?? 1,
      currencyId: aim.currency?.id != null ? String(aim.currency.id) : '',
    };
    return (
      formData.name !== currentForm.name ||
      formData.amount !== currentForm.amount ||
      formData.priority !== currentForm.priority ||
      formData.currencyId !== currentForm.currencyId
    );
  };

  const handleSelectAim = (aimId: number) => {
    if (isEditingView && isFormChanged(selectedAim)) {
      setIsEditingView(false);
    }
    setSelectedAimId(aimId);
  };

  const handleEditOpen = (aim: any) => {
    setFormData({
      name: aim.name ?? '',
      amount: aim.amount != null ? String(aim.amount) : '',
      priority: aim.priority ?? 1,
      currencyId: aim.currency?.id != null ? String(aim.currency.id) : '',
    });
    setEditErrors({});
    setPageError(null);
    setIsEditingView(true);
  };

  const handleCreateSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateAim('create')) return;
    try {
      const created = await createMutation.mutateAsync({
        data: {
          name: formData.name,
          amount: Number(formData.amount),
          priority: Number(formData.priority),
          currencyId: Number(formData.currencyId),
        },
      });
      setIsCreateModalOpen(false);
      setFormData(EMPTY_FORM);
      setCreateErrors({});
      setPageError(null);
      setSelectedAimId((created as any)?.id ?? null);
      invalidateAims();
    } catch (error) {
      setPageError(getErrorMessage(error));
    }
  };

  const handleUpdateSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedAim?.id || !validateAim('edit')) return;
    try {
      await updateMutation.mutateAsync({
        id: selectedAim.id,
        data: {
          name: formData.name,
          amount: Number(formData.amount),
          priority: Number(formData.priority),
          currencyId: Number(formData.currencyId),
        },
      });
      setIsEditingView(false);
      setEditErrors({});
      setPageError(null);
      invalidateAims();
    } catch (error) {
      setPageError(getErrorMessage(error));
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Ви впевнені, що хочете видалити цю ціль?')) return;
    try {
      await deleteMutation.mutateAsync({ id });
      if (selectedAimId === id) setSelectedAimId(null);
      setPageError(null);
      invalidateAims();
    } catch (error) {
      setPageError(getErrorMessage(error));
    }
  };

  const handleAddSources = async (sourceIds: number[]) => {
    if (!selectedAim?.id || sourceIds.length === 0) return;
    try {
      setDetailError(null);
      
      const errors: { sourceId: number; sourceName: string; error: string }[] = [];
      
      // Add sources sequentially
      for (const sourceId of sourceIds) {
        try {
          await addSourceMutation.mutateAsync({ aimId: selectedAim.id, sourceId });
        } catch (error) {
          const sourceName = activeSources.find((s) => s.id === sourceId)?.name || `ID: ${sourceId}`;
          errors.push({
            sourceId,
            sourceName,
            error: getErrorMessage(error),
          });
        }
      }

      setIsSourceSelectionModalOpen(false);
      invalidateAims();

      // Show errors if any
      if (errors.length > 0) {
        const errorMessage = errors
          .map((e) => `${e.sourceName}: ${e.error}`)
          .join('\n');
        setDetailError(`Помилки при додаванні джерел:\n${errorMessage}`);
      }
    } catch (error) {
      setDetailError(getErrorMessage(error));
    }
  };

  const handleRemoveSource = async (sourceId: number) => {
    if (!selectedAim?.id) return;
    try {
      setDetailError(null);
      await removeSourceMutation.mutateAsync({ aimId: selectedAim.id, sourceId });
      invalidateAims();
    } catch (error) {
      setDetailError(getErrorMessage(error));
    }
  };

  const handleDragEnd = async (event: DragEndEvent) => {
    const { active, over } = event;

    if (!over || active.id === over.id) return;

    // Find the dragged aim and the drop target aim
    const draggedAim = sortedAims.find((a) => a.id === active.id);
    const targetAim = sortedAims.find((a) => a.id === over.id);

    if (!draggedAim || !targetAim) return;

    try {
      const draggedIndex = sortedAims.findIndex((a) => a.id === active.id);
      const targetIndex = sortedAims.findIndex((a) => a.id === over.id);

      // If dropped on the same position, don't do anything
      if (draggedIndex === targetIndex) return;

      // Get the target priority (where we want to drop)
      const newPriority = targetAim.priority ?? 1;
      const oldPriority = draggedAim.priority ?? 1;

      // Swap priorities: dragged aim gets target's priority
      // All aims between old and new position shift their priorities
      const updatesToExecute: Array<{ id: number; newPrio: number }> = [];

      if (draggedIndex > targetIndex) {
        // Moving up (вверх): все від target до dragged мають +1
        // draggedAim gets targetAim.priority
        updatesToExecute.push({ id: draggedAim.id, newPrio: newPriority });

        // Shift all aims between target and dragged
        for (const aim of sortedAims) {
          const p = aim.priority ?? 1;
          if (p >= newPriority && p < oldPriority && aim.id !== draggedAim.id) {
            updatesToExecute.push({ id: aim.id, newPrio: p + 1 });
          }
        }
      } else {
        // Moving down (вниз): все від dragged до target мають -1
        // draggedAim gets targetAim.priority
        updatesToExecute.push({ id: draggedAim.id, newPrio: newPriority });

        // Shift all aims between dragged and target
        for (const aim of sortedAims) {
          const p = aim.priority ?? 1;
          if (p > oldPriority && p <= newPriority && aim.id !== draggedAim.id) {
            updatesToExecute.push({ id: aim.id, newPrio: p - 1 });
          }
        }
      }

      // Execute all updates
      for (const update of updatesToExecute) {
        const aimToUpdate = sortedAims.find((a) => a.id === update.id);
        if (aimToUpdate) {
          await updateMutation.mutateAsync({
            id: aimToUpdate.id,
            data: {
              name: aimToUpdate.name,
              amount: Number(aimToUpdate.amount),
              priority: update.newPrio,
              currencyId: Number(aimToUpdate.currency?.id),
            },
          });
        }
      }

      invalidateAims();
    } catch (error) {
      setPageError(getErrorMessage(error));
    }
  };

  if (isLoading) {
    return (
      <div className="w-full min-h-[calc(100vh-3rem)] space-y-6">
        <div className="flex items-center justify-between gap-4">
          <Skeleton className="h-10 w-56 rounded-xl" />
          <Skeleton className="h-11 w-36 rounded-xl" />
        </div>
        <div className="grid gap-6 xl:grid-cols-[minmax(0,1.5fr)_minmax(360px,0.8fr)]">
          <div className="space-y-4">
            <Skeleton className="h-44 rounded-2xl" />
            <Skeleton className="h-44 rounded-2xl" />
            <Skeleton className="h-44 rounded-2xl" />
          </div>
          <Skeleton className="h-[560px] rounded-2xl" />
        </div>
      </div>
    );
  }

  return (
    <div className="w-full min-h-[calc(100vh-3rem)] space-y-6">
      <div className="flex flex-col gap-3 lg:flex-row lg:items-end lg:justify-between">
        <div className="space-y-2">
          <h1 className="text-3xl font-semibold text-ink">Мої цілі</h1>
        </div>
        <Button onClick={handleCreateOpen}>+ Нова ціль</Button>
      </div>

      {pageError && (
        <div className="rounded-2xl border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {pageError}
        </div>
      )}

      <div className="grid gap-6 grid-cols-1 lg:grid-cols-[minmax(0,1.5fr)_minmax(360px,0.8fr)] items-start">
        <section className="space-y-4 order-2 lg:order-1">

          {sortedAims.length > 0 ? (
            <DndContext
              sensors={sensors}
              collisionDetection={closestCenter}
              onDragEnd={handleDragEnd}
            >
              <SortableContext
                items={sortedAims.map((a) => a.id)}
                strategy={verticalListSortingStrategy}
              >
                <div className="flex flex-col gap-4">
                  {sortedAims.map((aim) => (
                    <div
                      key={aim.id}
                      className="w-full"
                      onClick={() => handleSelectAim(aim.id)}
                    >
                      <AimProgressCard
                        aim={aim as any}
                        onEdit={handleEditOpen}
                        onDelete={handleDelete}
                      />
                    </div>
                  ))}
                </div>
              </SortableContext>
            </DndContext>
          ) : (
            <EmptyState
              title="Немає цілей"
              description="Створіть першу ціль, щоб бачити прогрес, джерела та пріоритети."
              action={<Button onClick={handleCreateOpen}>+ Нова ціль</Button>}
            />
          )}
        </section>

        <aside className="order-1 lg:order-2 xl:sticky xl:top-6">
          <div className="rounded-2xl border border-hairline bg-white p-5 shadow-sm">
            {selectedAim ? (
              <div className="space-y-5">
                {!isEditingView ? (
                  <div className="space-y-3">
                    <div className="flex items-start gap-3 justify-between">
                      <div className="flex-1">
                        <div className="flex flex-wrap gap-2 mb-2">
                          <span className={`inline-flex items-center rounded-full px-3 py-1 text-[12px] font-semibold ${selectedAim.isClosed ? 'bg-hairline text-inkMuted48' : 'bg-primary/10 text-primary'}`}>
                            {selectedAim.isClosed ? 'Закрита' : 'Активна'}
                          </span>
                        </div>
                        <h2 className="text-2xl font-semibold text-ink">{selectedAim.name}</h2>
                      </div>

                      <div className="flex flex-col gap-2 shrink-0">
                        <Button variant="secondary" size="sm" onClick={() => handleEditOpen(selectedAim)}>
                          Редагувати
                        </Button>
                        <Button onClick={() => selectedAim.id && handleDelete(selectedAim.id)} variant="danger" size="sm">
                          Видалити
                        </Button>
                      </div>
                    </div>

                    <div className="grid grid-cols-2 gap-3 text-sm">
                      <div className="rounded-xl border border-hairline bg-[#fafafc] p-3">
                        <div className="text-[#7a7a7a]">Сума</div>
                        <div className="mt-1 font-semibold text-ink">
                          {Number(selectedAim.amount ?? 0).toFixed(2)} {selectedAim.currency?.name}
                        </div>
                      </div>
                      <div className="rounded-xl border border-hairline bg-[#fafafc] p-3">
                        <div className="text-[#7a7a7a]">Зібрано</div>
                        <div className="mt-1 font-semibold text-ink">
                          {Number(selectedAim.progress?.collectedAmount ?? 0).toFixed(2)} {selectedAim.currency?.name}
                        </div>
                      </div>
                    </div>

                    <div className="h-2 overflow-hidden rounded-full bg-hairline">
                      <div
                        className="h-full rounded-full bg-primary transition-all duration-500"
                        style={{ width: `${Math.min(Number(selectedAim.progress?.completionPercentage ?? 0), 100)}%` }}
                      />
                    </div>
                    <div className="flex items-center justify-between text-xs text-inkMuted48">
                      <span>{Number(selectedAim.progress?.completionPercentage ?? 0).toFixed(2)}%</span>
                      <span>{selectedAim.currency?.name}</span>
                    </div>
                  </div>
                ) : (
                  <form onSubmit={handleUpdateSubmit} className="space-y-4">
                    <div>
                      <label className="block text-sm font-semibold text-ink mb-2">Назва</label>
                      <input
                        type="text"
                        value={formData.name}
                        onChange={(e) => {
                          setFormData({ ...formData, name: e.target.value });
                          if (editErrors.name) setEditErrors({ ...editErrors, name: undefined });
                        }}
                        className="w-full rounded-lg border border-hairline px-3 py-2 text-ink focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary/20"
                      />
                      {editErrors.name && <p className="mt-1 text-xs text-red-500">{editErrors.name}</p>}
                    </div>

                    <div>
                      <label className="block text-sm font-semibold text-ink mb-2">Цільова сума</label>
                      <input
                        type="number"
                        value={formData.amount}
                        step="0.01"
                        min="0.01"
                        onChange={(e) => {
                          setFormData({ ...formData, amount: e.target.value });
                          if (editErrors.amount) setEditErrors({ ...editErrors, amount: undefined });
                        }}
                        className="w-full rounded-lg border border-hairline px-3 py-2 text-ink focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary/20"
                      />
                      {editErrors.amount && <p className="mt-1 text-xs text-red-500">{editErrors.amount}</p>}
                    </div>

                    <div>
                      <label className="block text-sm font-semibold text-ink mb-2">Пріоритет</label>
                      <input
                        type="number"
                        value={formData.priority}
                        min="1"
                        onChange={(e) => {
                          setFormData({ ...formData, priority: Number(e.target.value) });
                          if (editErrors.priority) setEditErrors({ ...editErrors, priority: undefined });
                        }}
                        className="w-full rounded-lg border border-hairline px-3 py-2 text-ink focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary/20"
                      />
                      {editErrors.priority && <p className="mt-1 text-xs text-red-500">{editErrors.priority}</p>}
                    </div>

                    <div>
                      <label className="block text-sm font-semibold text-ink mb-2">Валюта</label>
                      <select
                        value={formData.currencyId}
                        onChange={(e) => {
                          setFormData({ ...formData, currencyId: e.target.value });
                          if (editErrors.currencyId) setEditErrors({ ...editErrors, currencyId: undefined });
                        }}
                        className="w-full rounded-lg border border-hairline px-3 py-2 text-ink focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary/20"
                      >
                        <option value="">Оберіть валюту</option>
                        {currencies.map((c: any) => (
                          <option key={c.id} value={c.id}>{c.name}</option>
                        ))}
                      </select>
                      {editErrors.currencyId && <p className="mt-1 text-xs text-red-500">{editErrors.currencyId}</p>}
                    </div>

                    <div className="flex gap-3">
                      <Button
                        variant="secondary"
                        onClick={() => setIsEditingView(false)}
                        type="button"
                        disabled={updateMutation.isPending}
                      >
                        Скасувати
                      </Button>
                      <Button
                        type="submit"
                        isLoading={updateMutation.isPending}
                      >
                        Зберегти
                      </Button>
                    </div>
                  </form>
                )}

                {!isEditingView && (
                  <div className="space-y-3 border-t border-hairline pt-5">
                    <div className="flex items-center justify-between gap-3">
                      <h3 className="text-sm font-semibold text-ink">Прив'язані джерела</h3>
                      <span className="text-xs text-inkMuted48">{(selectedAim.sources ?? []).length} шт.</span>
                    </div>

                    {detailError && (
                      <div className="rounded-xl border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700 whitespace-pre-line">
                        {detailError}
                      </div>
                    )}

                    {(selectedAim.sources ?? []).length > 0 ? (
                      <div className="flex flex-wrap gap-2">
                        {(selectedAim.sources ?? []).map((source: any) => (
                          <div
                            key={source.id}
                            className="inline-flex items-center gap-2 px-3 py-1.5 rounded-lg bg-primary/10 border border-primary/20 hover:bg-primary/15 transition-colors cursor-default group"
                            title={source.name}
                          >
                            <span className="text-xs font-medium text-primary truncate">
                              {source.name}
                            </span>
                            {source.isArchived && (
                              <span className="rounded-full bg-amber-100 px-1.5 text-[10px] font-semibold text-amber-700">
                                архив
                              </span>
                            )}
                            <button
                              onClick={(e) => {
                                e.stopPropagation();
                                source.id && handleRemoveSource(source.id);
                              }}
                              className="opacity-100 sm:opacity-0 sm:group-hover:opacity-100 transition-opacity text-primary/60 hover:text-primary ml-1 p-1 touch-manipulation"
                              title="Видалити"
                            >
                              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                                <line x1="18" y1="6" x2="6" y2="18" />
                                <line x1="6" y1="6" x2="18" y2="18" />
                              </svg>
                            </button>
                          </div>
                        ))}
                      </div>
                    ) : (
                      <div className="rounded-xl border border-dashed border-hairline bg-[#fafafc] px-4 py-5 text-sm text-inkMuted48">
                        До цієї цілі ще не прив'язано жодного джерела.
                      </div>
                    )}

                    <Button
                      onClick={() => setIsSourceSelectionModalOpen(true)}
                      disabled={availableSources.length === 0}
                      className="w-full"
                      variant="secondary"
                    >
                      {availableSources.length === 0
                        ? 'Немає доступних джерел'
                        : `+ Вибрати джерела (${availableSources.length})`}
                    </Button>
                  </div>
                )}
              </div>
            ) : (
              <div className="flex min-h-[420px] items-center justify-center rounded-xl border border-dashed border-hairline bg-[#fafafc] px-6 text-center text-sm text-inkMuted48">
                Оберіть ціль зі списку, щоб побачити деталі, джерела та керування.
              </div>
            )}
          </div>
        </aside>
      </div>

      <Modal isOpen={isCreateModalOpen} title="Нова ціль" onClose={() => setIsCreateModalOpen(false)}>
        <form onSubmit={handleCreateSubmit}>
          <AimForm
            formData={formData}
            onFormChange={setFormData}
            errors={createErrors}
            onErrorChange={setCreateErrors}
            currencies={currencies}
            submitLabel="Створити"
            isSubmitting={createMutation.isPending}
            onClose={() => setIsCreateModalOpen(false)}
          />
        </form>
      </Modal>

      <SourceSelectionModal
        isOpen={isSourceSelectionModalOpen}
        onClose={() => setIsSourceSelectionModalOpen(false)}
        sources={activeSources}
        linkedSourceIds={linkedSourceIds}
        onConfirm={handleAddSources}
        isLoading={addSourceMutation.isPending}
      />
    </div>
  );
};
