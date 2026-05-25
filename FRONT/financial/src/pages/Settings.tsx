import React, { useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { Button } from '../components/Button';
import { Modal } from '../components/Modal';
import { ConfirmModal } from '../components/ConfirmModal';
import { Skeleton } from '../components/Skeleton';
import { EmptyState } from '../components/EmptyState';
import { Pencil, Trash2, FolderOpen } from 'lucide-react';
import {
  useGetApiCategory,
  usePostApiCategory,
  usePatchApiCategoryId,
  useDeleteApiCategoryId,
  useGetApiFrequency,
  usePostApiFrequency,
  usePatchApiFrequencyId,
  useDeleteApiFrequencyId,
  useGetApiIntervalUnit,
  useGetApiTransaction,
  usePostApiTransaction,
  useGetApiSource,
  useGetApiTransactionType,
} from '../api/generated/endpoints';
import { TransactionTable } from '../components/TransactionTable';
import { translateIntervalUnitName, getFrequencyLabel } from '../utils/display-helpers';

export const Settings: React.FC = () => {
  const queryClient = useQueryClient();

  // Categories state (copied from Categories page)
  const [isCategoryModalOpen, setIsCategoryModalOpen] = useState(false);
  const [editingCategory, setEditingCategory] = useState<{ id: number; name: string } | null>(null);
  const [categoryToDelete, setCategoryToDelete] = useState<number | null>(null);
  const [categoryName, setCategoryName] = useState('');
  const [categoryError, setCategoryError] = useState('');

  const categoriesQuery = useGetApiCategory();
  const createCategory = usePostApiCategory();
  const updateCategory = usePatchApiCategoryId();
  const deleteCategory = useDeleteApiCategoryId();

  const categories = Array.isArray(categoriesQuery.data) ? categoriesQuery.data : [];
  const isCategoriesLoading = categoriesQuery.isLoading;

  // Frequencies state
  const [isFreqModalOpen, setIsFreqModalOpen] = useState(false);
  const [editingFreq, setEditingFreq] = useState<any | null>(null);
  const [freqToDelete, setFreqToDelete] = useState<number | null>(null);
  const [freqName, setFreqName] = useState('');
  const [freqValue, setFreqValue] = useState<number | undefined>(undefined);
  const [freqUnitId, setFreqUnitId] = useState<number | undefined>(undefined);
  const [isOneTime, setIsOneTime] = useState(false);
  const [freqError, setFreqError] = useState('');

  const freqsQuery = useGetApiFrequency();
  const createFreq = usePostApiFrequency();
  const updateFreq = usePatchApiFrequencyId();
  const deleteFreq = useDeleteApiFrequencyId();
  const unitsQuery = useGetApiIntervalUnit();

  const freqs = Array.isArray(freqsQuery.data) ? freqsQuery.data : [];
  const units = Array.isArray(unitsQuery.data) ? unitsQuery.data : [];
  const isFreqsLoading = freqsQuery.isLoading || unitsQuery.isLoading;

  // Transactions inside settings
  const [expandCategories, setExpandCategories] = useState(true);
  const [expandTransactions, setExpandTransactions] = useState(false);
  const [expandFrequencies, setExpandFrequencies] = useState(false);

  const transactionsQuery = useGetApiTransaction({ Limit: 50 });
  const sourcesQuery = useGetApiSource();
  const typesQuery = useGetApiTransactionType();
  const createTransaction = usePostApiTransaction();
  const transactions = Array.isArray(transactionsQuery.data?.data)
    ? transactionsQuery.data.data
    : Array.isArray(transactionsQuery.data)
    ? transactionsQuery.data
    : [];
  const sources = Array.isArray(sourcesQuery.data) ? sourcesQuery.data : [];
  const types = Array.isArray(typesQuery.data) ? typesQuery.data : [];

  const [isCreateTxModalOpen, setIsCreateTxModalOpen] = useState(false);
  const [txForm, setTxForm] = useState({ amount: '', date: '', sourceId: '', transactionTypeId: '', categoryId: '', comment: '' });
  const [txErrors, setTxErrors] = useState<any>({});

  const getLocalDatetime = (d?: string) => {
    const dt = d ? new Date(d) : new Date();
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${dt.getFullYear()}-${pad(dt.getMonth()+1)}-${pad(dt.getDate())}T${pad(dt.getHours())}:${pad(dt.getMinutes())}:${pad(dt.getSeconds())}`;
  };

  const openCreateTx = () => {
    setTxErrors({});
    setTxForm({ amount: '', date: getLocalDatetime(), sourceId: '', transactionTypeId: '', categoryId: '', comment: '' });
    setIsCreateTxModalOpen(true);
  };

  const validateTx = () => {
    const errs: any = {};
    if (!txForm.date) errs.date = 'Оберіть дату';
    if (!txForm.sourceId) errs.sourceId = 'Виберіть джерело';
    if (!txForm.transactionTypeId) errs.transactionTypeId = 'Виберіть тип транзакції';
    if (!txForm.amount || Number(txForm.amount) <= 0) errs.amount = 'Сума має бути більшою за 0';
    setTxErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const submitCreateTx = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateTx()) return;
    try {
      const source = sources.find((s:any)=>s.id===Number(txForm.sourceId));
      await createTransaction.mutateAsync({ data: { amount: Number(txForm.amount), date: txForm.date, sourceId: Number(txForm.sourceId), transactionTypeId: Number(txForm.transactionTypeId), categoryId: txForm.categoryId ? Number(txForm.categoryId) : null, comment: txForm.comment || '', currencyId: source?.currency?.id } });
      setIsCreateTxModalOpen(false);
      queryClient.invalidateQueries({ queryKey: ['/api/Transaction'] });
      transactionsQuery.refetch();
    } catch (err) {
      console.error(err);
    }
  };

  // Category handlers
  const handleOpenCreateCategory = () => {
    setEditingCategory(null);
    setCategoryName('');
    setCategoryError('');
    setIsCategoryModalOpen(true);
  };
  const handleOpenEditCategory = (category: any) => {
    setEditingCategory({ id: category.id, name: category.name });
    setCategoryName(category.name);
    setCategoryError('');
    setIsCategoryModalOpen(true);
  };
  const handleSubmitCategory = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!categoryName.trim()) {
      setCategoryError('Назва категорії не може бути порожньою');
      return;
    }
    try {
      if (editingCategory) {
        await updateCategory.mutateAsync({ id: editingCategory.id, data: { name: categoryName.trim() } });
      } else {
        await createCategory.mutateAsync({ data: { name: categoryName.trim() } });
      }
      setIsCategoryModalOpen(false);
      queryClient.invalidateQueries({ queryKey: ['/api/Category'] });
    } catch (err: any) {
      console.error('Error saving category:', err);
      setCategoryError('Помилка збереження категорії');
    }
  };
  const handleDeleteCategory = (id: number) => setCategoryToDelete(id);
  const confirmDeleteCategory = async () => {
    if (!categoryToDelete) return;
    try {
      await deleteCategory.mutateAsync({ id: categoryToDelete });
      queryClient.invalidateQueries({ queryKey: ['/api/Category'] });
    } catch (err) {
      console.error('Error deleting category:', err);
      alert('Помилка видалення категорії. Можливо вона використовується.');
    } finally { setCategoryToDelete(null); }
  };

  // Frequency handlers
  const handleOpenCreateFreq = () => {
    setEditingFreq(null);
    setFreqName('');
    setFreqValue(undefined);
    setFreqUnitId(undefined);
    setIsOneTime(false);
    setFreqError('');
    setIsFreqModalOpen(true);
  };
  const handleOpenEditFreq = (f: any) => {
    setEditingFreq(f);
    setFreqName(f.name || '');
    setFreqValue(f.intervalValue ?? undefined);
    setFreqUnitId(f.intervalUnit?.id ?? undefined);
    setIsOneTime((f.intervalValue ?? 0) >= 9999);
    setFreqError('');
    setIsFreqModalOpen(true);
  };
  const handleSubmitFreq = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!freqName.trim()) {
      setFreqError('Назва проміжку не може бути порожньою');
      return;
    }
    try {
      const payload: any = { name: freqName.trim() };
      if (isOneTime) {
        // backend requires intervalUnitId; choose 'year' unit if available else first unit
        const yearUnit = units.find((u: any) => (u.name || '').toString().toLowerCase().includes('year') || (u.name || '').toString().toLowerCase().includes('рік'));
        const chosenUnitId = yearUnit?.id ?? units[0]?.id;
        if (chosenUnitId) payload.intervalUnitId = chosenUnitId;
        // large value to approximate "one-time" (effectively won't repeat in practical use)
        payload.intervalValue = 9999;
      } else {
        if (freqUnitId) payload.intervalUnitId = freqUnitId;
        if (typeof freqValue === 'number') payload.intervalValue = freqValue;
      }

      if (editingFreq) {
        await updateFreq.mutateAsync({ id: editingFreq.id, data: payload });
      } else {
        await createFreq.mutateAsync({ data: payload });
      }

      setIsFreqModalOpen(false);
      queryClient.invalidateQueries({ queryKey: ['/api/Frequency'] });
    } catch (err: any) {
      console.error('Error saving frequency:', err);
      setFreqError('Помилка збереження проміжку');
    }
  };
  const handleDeleteFreq = (id: number) => setFreqToDelete(id);
  const confirmDeleteFreq = async () => {
    if (!freqToDelete) return;
    try {
      await deleteFreq.mutateAsync({ id: freqToDelete });
      queryClient.invalidateQueries({ queryKey: ['/api/Frequency'] });
    } catch (err) {
      console.error('Error deleting frequency:', err);
      alert('Помилка видалення проміжку.');
    } finally { setFreqToDelete(null); }
  };

  if (isCategoriesLoading && isFreqsLoading) {
    return (
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <Skeleton className="h-8 w-48" />
          <Skeleton className="h-10 w-32 rounded-lg" />
        </div>
        <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-4 gap-4">
          <Skeleton className="h-24 rounded-2xl" />
          <Skeleton className="h-24 rounded-2xl" />
          <Skeleton className="h-24 rounded-2xl" />
          <Skeleton className="h-24 rounded-2xl" />
        </div>
      </div>
    );
  }

  return (
  return (
    <div className="w-full space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-ink">Налаштування</h1>
          <p className="text-sm text-[#7a7a7a] mt-1">Керування категоріями та транзакціями</p>
        </div>
      </div>

      {/* Categories accordion */}
      <div className="bg-white border border-hairline rounded-2xl p-4">
        <button className="w-full flex items-center justify-between" onClick={() => setExpandCategories((s) => !s)}>
          <div className="flex items-center gap-3">
            <div className="bg-primary/10 text-primary p-2 rounded-lg"><FolderOpen size={18} /></div>
            <div>
              <div className="font-semibold">Категорії</div>
              <div className="text-xs text-[#7a7a7a]">Створюйте та видаляйте категорії</div>
            </div>
          </div>
          <div className="flex items-center gap-3">
            <Button onClick={(e)=>{ e.stopPropagation(); handleOpenCreateCategory(); }}>+ Додати категорію</Button>
            <div className="text-sm text-[#7a7a7a]">{expandCategories ? 'Згорнути' : 'Розгорнути'}</div>
          </div>
        </button>

        {expandCategories && (
          <div className="mt-4">
            {categories.length > 0 ? (
              <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                {categories.map((category: any) => (
                  <div key={category.id} className="bg-white border border-hairline rounded-2xl p-5 shadow-sm hover:shadow-md transition-shadow group relative overflow-hidden">
                    <div className="flex items-center gap-3 mb-3">
                      <div className="bg-primary/10 text-primary p-2.5 rounded-xl shrink-0">
                        <FolderOpen size={20} />
                      </div>
                      <h3 className="font-semibold text-ink text-base truncate" title={category.name}>{category.name}</h3>
                    </div>

                    <div className="flex gap-2 justify-end mt-4 pt-4 border-t border-hairline">
                      <button onClick={() => handleDeleteCategory(category.id)} className="text-[#7a7a7a] hover:text-red-500 transition-colors p-1.5 rounded-lg hover:bg-red-50 flex items-center justify-center" title="Видалити"><Trash2 size={16} /></button>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <EmptyState title="Немає категорій" description="Створіть свою першу категорію для кращої аналітики." action={<Button onClick={handleOpenCreateCategory}>+ Нова категорія</Button>} />
            )}
          </div>
        )}
      </div>

      {/* Transactions accordion */}
      <div className="bg-white border border-hairline rounded-2xl p-4">
        <button className="w-full flex items-center justify-between" onClick={() => setExpandTransactions((s) => !s)}>
          <div className="flex items-center gap-3">
            <div className="bg-primary/10 text-primary p-2 rounded-lg"><FolderOpen size={18} /></div>
            <div>
              <div className="font-semibold">Транзакції</div>
              <div className="text-xs text-[#7a7a7a]">Перегляд останніх транзакцій та додавання нових</div>
            </div>
          </div>
          <div className="flex items-center gap-3">
            <Button onClick={(e)=>{ e.stopPropagation(); openCreateTx(); }}>+ Додати транзакцію</Button>
            <div className="text-sm text-[#7a7a7a]">{expandTransactions ? 'Згорнути' : 'Розгорнути'}</div>
          </div>
        </button>

        {expandTransactions && (
          <div className="mt-4">
            {transactions.length > 0 ? (
              <TransactionTable transactions={transactions as any} onDelete={(id)=>{ deleteMutation.mutateAsync({ id }).then(()=>transactionsQuery.refetch()); }} />
            ) : (
              <EmptyState title="Немає транзакцій" description="Додайте першу транзакцію" action={<Button onClick={openCreateTx}>+ Нова транзакція</Button>} />
            )}
          </div>
        )}
      </div>

      {/* Frequencies accordion (optional) */}
      <div className="bg-white border border-hairline rounded-2xl p-4">
        <button className="w-full flex items-center justify-between" onClick={() => setExpandFrequencies((s)=>!s)}>
          <div className="flex items-center gap-3">
            <div className="bg-primary/10 text-primary p-2 rounded-lg"><FolderOpen size={18} /></div>
            <div>
              <div className="font-semibold">Проміжки (Інтервали)</div>
              <div className="text-xs text-[#7a7a7a]">Управління проміжками повторення</div>
            </div>
          </div>
          <div className="text-sm text-[#7a7a7a]">{expandFrequencies ? 'Згорнути' : 'Розгорнути'}</div>
        </button>

        {expandFrequencies && (
          <div className="mt-4">
            {freqs.length > 0 ? (
              <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                {freqs.map((f: any) => (
                  <div key={f.id} className="bg-white border border-hairline rounded-2xl p-5 shadow-sm hover:shadow-md transition-shadow group relative overflow-hidden">
                    <div className="flex items-center gap-3 mb-3">
                      <div className="bg-primary/10 text-primary p-2.5 rounded-xl shrink-0">
                        <FolderOpen size={20} />
                      </div>
                      <div className="min-w-0">
                        <h3 className="font-semibold text-ink text-base truncate" title={f.name || ''}>{getFrequencyLabel(f)}</h3>
                        <div className="text-xs text-[#7a7a7a] mt-1">
                          {f.name ? (f.userId === 0 ? getFrequencyLabel(f) : f.name) : (f.intervalUnit?.name ? `${f.intervalValue ?? ''} ${translateIntervalUnitName(f.intervalUnit?.name)}` : 'Одноразовий')}
                        </div>
                      </div>
                    </div>

                    <div className="flex gap-2 justify-end mt-4 pt-4 border-t border-hairline">
                      <button onClick={() => handleDeleteFreq(f.id)} className="text-[#7a7a7a] hover:text-red-500 transition-colors p-1.5 rounded-lg hover:bg-red-50 flex items-center justify-center" title="Видалити"><Trash2 size={16} /></button>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <EmptyState title="Немає проміжків" description="Створіть свій перший проміжок" action={<Button onClick={handleOpenCreateFreq}>+ Новий проміжок</Button>} />
            )}
          </div>
        )}
      </div>

      {/* Category Modal */}
      <Modal isOpen={isCategoryModalOpen} title={editingCategory ? 'Редагувати категорію' : 'Нова категорія'} onClose={() => setIsCategoryModalOpen(false)} size="sm">
        <form onSubmit={handleSubmitCategory} className="space-y-4">
          <div>
            <label className="block text-sm font-semibold text-ink mb-2">Назва категорії</label>
            <input type="text" value={categoryName} onChange={(e) => { setCategoryName(e.target.value); setCategoryError(''); }} placeholder="Наприклад: Їжа, Транспорт..." className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]" autoFocus />
            {categoryError && <p className="mt-1 text-xs text-red-500">{categoryError}</p>}
          </div>

          <div className="flex gap-3 pt-4">
            <Button variant="secondary" onClick={() => setIsCategoryModalOpen(false)} type="button">Скасувати</Button>
            <Button type="submit" isLoading={createCategory.isPending || updateCategory.isPending}>{editingCategory ? 'Зберегти' : 'Створити'}</Button>
          </div>
        </form>
      </Modal>

      <ConfirmModal isOpen={categoryToDelete !== null} title="Видалення категорії" message="Ви впевнені, що хочете видалити цю категорію?" onConfirm={confirmDeleteCategory} onCancel={() => setCategoryToDelete(null)} />

      {/* Frequency Modal */}
      <Modal isOpen={isFreqModalOpen} title={editingFreq ? 'Редагувати проміжок' : 'Новий проміжок'} onClose={() => setIsFreqModalOpen(false)} size="sm">
        <form onSubmit={handleSubmitFreq} className="space-y-4">
          <div>
            <label className="block text-sm font-semibold text-ink mb-2">Назва проміжку</label>
            <input type="text" value={freqName} onChange={(e) => { setFreqName(e.target.value); setFreqError(''); }} placeholder="Наприклад: Щотижня, Щомісяця або власна назва" className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]" autoFocus />
          </div>

          <div className="flex items-center gap-3">
            <input id="oneTime" type="checkbox" checked={isOneTime} onChange={(e) => setIsOneTime(e.target.checked)} />
            <label htmlFor="oneTime" className="text-sm">Одноразовий проміжок</label>
          </div>

          {!isOneTime && (
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-sm font-semibold text-ink mb-2">Значення інтервалу</label>
                <input type="number" min={1} value={freqValue ?? ''} onChange={(e) => setFreqValue(e.target.value ? Number(e.target.value) : undefined)} placeholder="Наприклад: 1, 2, 3" className="w-full px-4 py-2 border border-hairline rounded-lg" />
              </div>
              <div>
                <label className="block text-sm font-semibold text-ink mb-2">Одиниця інтервалу</label>
                <select value={freqUnitId ?? ''} onChange={(e) => setFreqUnitId(e.target.value ? Number(e.target.value) : undefined)} className="w-full px-4 py-2 border border-hairline rounded-lg">
                  <option value="">Оберіть одиницю...</option>
                  {units.map((u: any) => (<option key={u.id} value={u.id}>{u.name}</option>))}
                </select>
              </div>
            </div>
          )}

          {freqError && <p className="mt-1 text-xs text-red-500">{freqError}</p>}

          <div className="flex gap-3 pt-4">
            <Button variant="secondary" onClick={() => setIsFreqModalOpen(false)} type="button">Скасувати</Button>
            <Button type="submit" isLoading={createFreq.isPending || updateFreq.isPending}>{editingFreq ? 'Зберегти' : 'Створити'}</Button>
          </div>
        </form>
      </Modal>

      <ConfirmModal isOpen={freqToDelete !== null} title="Видалення проміжку" message="Ви впевнені, що хочете видалити цей проміжок?" onConfirm={confirmDeleteFreq} onCancel={() => setFreqToDelete(null)} />

      {/* Create transaction modal */}
      <Modal isOpen={isCreateTxModalOpen} title="Нова транзакція" onClose={() => setIsCreateTxModalOpen(false)} size="md">
        <form onSubmit={submitCreateTx} className="space-y-4">
          <div>
            <label className="block text-sm font-semibold text-ink mb-2">Тип</label>
            <select value={txForm.transactionTypeId} onChange={(e)=>setTxForm({...txForm, transactionTypeId: e.target.value})} className="w-full px-4 py-2 border border-hairline rounded-lg">
              <option value="">Виберіть тип</option>
              {types.map((t:any)=>(<option key={t.id} value={t.id}>{t.name}</option>))}
            </select>
            {txErrors.transactionTypeId && <p className="mt-1 text-xs text-red-500">{txErrors.transactionTypeId}</p>}
          </div>

          <div>
            <label className="block text-sm font-semibold text-ink mb-2">Сума</label>
            <input type="number" value={txForm.amount} onChange={(e)=>setTxForm({...txForm, amount: e.target.value})} placeholder="0.00" step="0.01" min="0.01" className="w-full px-4 py-2 border border-hairline rounded-lg" />
            {txErrors.amount && <p className="mt-1 text-xs text-red-500">{txErrors.amount}</p>}
          </div>

          <div>
            <label className="block text-sm font-semibold text-ink mb-2">Дата</label>
            <input type="datetime-local" step="1" value={txForm.date} onChange={(e)=>setTxForm({...txForm, date: e.target.value})} className="w-full px-4 py-2 border border-hairline rounded-lg" />
            {txErrors.date && <p className="mt-1 text-xs text-red-500">{txErrors.date}</p>}
          </div>

          <div>
            <label className="block text-sm font-semibold text-ink mb-2">Джерело</label>
            <select value={txForm.sourceId} onChange={(e)=>setTxForm({...txForm, sourceId: e.target.value})} className="w-full px-4 py-2 border border-hairline rounded-lg">
              <option value="">Виберіть джерело</option>
              {sources.map((s:any)=>(<option key={s.id} value={s.id}>{s.name}</option>))}
            </select>
            {txErrors.sourceId && <p className="mt-1 text-xs text-red-500">{txErrors.sourceId}</p>}
          </div>

          <div>
            <label className="block text-sm font-semibold text-ink mb-2">Категорія (опціонально)</label>
            <select value={txForm.categoryId} onChange={(e)=>setTxForm({...txForm, categoryId: e.target.value})} className="w-full px-4 py-2 border border-hairline rounded-lg">
              <option value="">Без категорії</option>
              {categories.map((c:any)=>(<option key={c.id} value={c.id}>{c.name}</option>))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-semibold text-ink mb-2">Коментар (опціонально)</label>
            <textarea value={txForm.comment} onChange={(e)=>setTxForm({...txForm, comment: e.target.value})} rows={3} className="w-full px-4 py-2 border border-hairline rounded-lg" />
          </div>

          <div className="flex gap-3 pt-4">
            <Button variant="secondary" onClick={()=>setIsCreateTxModalOpen(false)} type="button">Скасувати</Button>
            <Button type="submit" isLoading={createTransaction.isPending}>Створити</Button>
          </div>
        </form>
      </Modal>
    </div>
  );
};

export default Settings;
