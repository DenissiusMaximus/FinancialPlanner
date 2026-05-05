import React, { useState } from 'react';
import { 
  useGetPlannedTransactionsQuery, 
  useCreatePlannedTransactionMutation,
  useUpdatePlannedTransactionMutation,
  useDeletePlannedTransactionMutation,
  useGetSourcesSummaryQuery,
  useGetCategoriesQuery,
  useGetTransactionTypesQuery,
  useGetFrequenciesQuery
} from '../../store/apiSlice';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../../components/ui/Card';
import { CalendarClock, Plus, Trash2, ArrowRightLeft, Pencil } from 'lucide-react';
import { Input } from '../../components/ui/Input';
import { Button } from '../../components/ui/Button';
import { Modal } from '../../components/ui/Modal';
import toast from 'react-hot-toast';
import { formatCurrency, toSafeNumber } from '../../utils/number';

type PlannedItem = {
  id: number;
  name?: string | null;
  amount?: number;
  startDate: string;
  source?: { id: number; name?: string | null } | null;
  category?: { id: number; name?: string | null } | null;
  transactionType?: { id: number; name?: string | null } | null;
  frequency?: { id: number; name?: string | null } | null;
};
type Lookup = { id: number; name?: string | null };

export function PlannedTransactions() {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  
  // Queries
  const { data: plannedTxsRaw, isLoading, error } = useGetPlannedTransactionsQuery();
  const { data: sourceSummary } = useGetSourcesSummaryQuery();
  const { data: categories } = useGetCategoriesQuery();
  const { data: transactionTypes } = useGetTransactionTypesQuery();
  const { data: frequencies } = useGetFrequenciesQuery();

  // /api/Source/summary returns { total, sources[] }
  const sources = (sourceSummary?.sources ?? []) as Lookup[];
  const plannedTxs = (plannedTxsRaw ?? []) as PlannedItem[];
  
  const [createPlannedTx, { isLoading: isCreating }] = useCreatePlannedTransactionMutation();
  const [updatePlannedTx, { isLoading: isUpdating }] = useUpdatePlannedTransactionMutation();
  const [deletePlannedTx] = useDeletePlannedTransactionMutation();

  // Form State
  const [name, setName] = useState('');
  const [amount, setAmount] = useState('');
  const [sourceId, setSourceId] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [transactionTypeId, setTransactionTypeId] = useState('');
  const [frequencyId, setFrequencyId] = useState('');
  const [startDate, setStartDate] = useState(new Date().toISOString().split('T')[0]);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [editName, setEditName] = useState('');
  const [editAmount, setEditAmount] = useState('');
  const [editSourceId, setEditSourceId] = useState('');
  const [editCategoryId, setEditCategoryId] = useState('');
  const [editTransactionTypeId, setEditTransactionTypeId] = useState('');
  const [editFrequencyId, setEditFrequencyId] = useState('');
  const [editStartDate, setEditStartDate] = useState(new Date().toISOString().split('T')[0]);

  const handleCreate = async (e) => {
    e.preventDefault();
    if (!name || !amount || !sourceId || !transactionTypeId || !frequencyId || !startDate) return;

    try {
      await createPlannedTx({
        name,
        amount: parseFloat(amount),
        startDate: new Date(startDate).toISOString().split('T')[0], // Format as date (YYYY-MM-DD)
        currencyId: 1, // Default currency
        transactionTypeId: parseInt(transactionTypeId),
        categoryId: categoryId ? parseInt(categoryId) : null,
        sourceId: parseInt(sourceId),
        frequencyId: parseInt(frequencyId)
      }).unwrap();
      
      toast.success('Subscription added successfully');
      setName('');
      setAmount('');
      setSourceId('');
      setCategoryId('');
      setTransactionTypeId('');
      setFrequencyId('');
      setIsModalOpen(false);
    } catch (err) {
      toast.error('Failed to add subscription');
      console.error(err);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this subscription?')) return;
    try {
      await deletePlannedTx(id).unwrap();
      toast.success('Subscription deleted');
    } catch (err) {
      toast.error('Failed to delete subscription');
      console.error(err);
    }
  };

  const openEditModal = (tx: PlannedItem) => {
    setEditingId(tx.id);
    setEditName(tx.name ?? '');
    setEditAmount(String(toSafeNumber(tx.amount)));
    setEditStartDate((tx.startDate ?? new Date().toISOString()).split('T')[0]);
    setEditSourceId(tx.source?.id ? String(tx.source.id) : '');
    setEditCategoryId(tx.category?.id ? String(tx.category.id) : '');
    setEditTransactionTypeId(tx.transactionType?.id ? String(tx.transactionType.id) : '');
    setEditFrequencyId(tx.frequency?.id ? String(tx.frequency.id) : '');
    setIsEditModalOpen(true);
  };

  const handleUpdate = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!editingId || !editName || !editAmount || !editSourceId || !editTransactionTypeId || !editFrequencyId || !editStartDate) return;

    try {
      await updatePlannedTx({
        id: editingId,
        name: editName,
        amount: parseFloat(editAmount),
        startDate: editStartDate,
        currencyId: 1,
        transactionTypeId: parseInt(editTransactionTypeId),
        categoryId: editCategoryId ? parseInt(editCategoryId) : null,
        sourceId: parseInt(editSourceId),
        frequencyId: parseInt(editFrequencyId),
      }).unwrap();

      toast.success('Subscription updated');
      setIsEditModalOpen(false);
      setEditingId(null);
    } catch (err) {
      toast.error('Failed to update subscription');
      console.error(err);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">Subscriptions</h1>
          <p className="text-sm text-slate-500 mt-1">Manage your planned and recurring transactions.</p>
        </div>
        <div className="mt-4 sm:mt-0">
          <Button onClick={() => setIsModalOpen(true)} className="flex items-center gap-2">
            <Plus className="h-4 w-4" /> Add Subscription
          </Button>
        </div>
      </div>

      <Card>
        <CardHeader className="border-b border-slate-100 pb-4">
          <CardTitle>Active Subscriptions</CardTitle>
          <CardDescription>Your recurring income and expenses.</CardDescription>
        </CardHeader>
        <CardContent className="p-0">
          {isLoading ? (
            <div className="text-center py-12 text-slate-500">Loading...</div>
          ) : error ? (
            <div className="text-center py-12 text-red-500">Failed to load subscriptions.</div>
          ) : plannedTxs?.length > 0 ? (
            <div className="divide-y divide-slate-100">
              {plannedTxs.map((tx) => {
                const amountValue = toSafeNumber(tx.amount);

                return (
                <div key={tx.id} className="flex items-center justify-between p-4 sm:px-6 hover:bg-slate-50 transition-colors">
                  <div className="flex items-center space-x-4">
                    <div className="p-2 rounded-full bg-blue-100 text-blue-600">
                      <ArrowRightLeft className="h-4 w-4" />
                    </div>
                    <div>
                      <p className="text-sm font-medium text-slate-900">{tx.name}</p>
                      <div className="flex items-center gap-2 text-xs text-slate-500">
                        <span>Starts: {new Date(tx.startDate).toLocaleDateString()}</span>
                        <span>•</span>
                        <span>{tx.frequency?.name || 'Recurring'}</span>
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center gap-4">
                    <div className="font-semibold text-slate-900">
                      ${formatCurrency(Math.abs(amountValue), { minimumFractionDigits: 2 })}
                    </div>
                    <button
                      onClick={() => openEditModal(tx)}
                      className="text-slate-400 hover:text-primary-600 transition-colors p-2 rounded-md hover:bg-primary-50"
                      title="Edit Subscription"
                    >
                      <Pencil className="h-4 w-4" />
                    </button>
                    <button 
                      onClick={() => handleDelete(tx.id)}
                      className="text-slate-400 hover:text-red-600 transition-colors p-2 rounded-md hover:bg-red-50"
                      title="Delete Subscription"
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  </div>
                </div>
                );
              })}
            </div>
          ) : (
            <div className="text-center py-16">
              <div className="inline-flex h-12 w-12 items-center justify-center rounded-full bg-slate-100 mb-4">
                <CalendarClock className="h-6 w-6 text-slate-400" />
              </div>
              <h3 className="text-sm font-medium text-slate-900">No subscriptions found</h3>
              <p className="text-sm text-slate-500 mt-1">Add your recurring payments like rent or Netflix.</p>
            </div>
          )}
        </CardContent>
      </Card>

      <Modal 
        isOpen={isModalOpen} 
        onClose={() => setIsModalOpen(false)} 
        title="Add New Subscription"
      >
        <form onSubmit={handleCreate} className="space-y-4">
          <Input
            label="Name"
            placeholder="e.g. Netflix, Salary"
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
            autoFocus
          />
          <Input
            label="Amount"
            type="number"
            step="0.01"
            placeholder="0.00"
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            required
          />
          <Input
            label="Start Date"
            type="date"
            value={startDate}
            onChange={(e) => setStartDate(e.target.value)}
            required
          />
          
          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Type</label>
            <select 
              className="flex h-10 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent shadow-sm"
              value={transactionTypeId}
              onChange={(e) => setTransactionTypeId(e.target.value)}
              required
            >
              <option value="">Select Type</option>
              {transactionTypes?.map(type => (
                <option key={type.id} value={type.id}>{type.name}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Source (Wallet)</label>
            <select 
              className="flex h-10 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent shadow-sm"
              value={sourceId}
              onChange={(e) => setSourceId(e.target.value)}
              required
            >
              <option value="">Select Source</option>
              {sources?.map(source => (
                <option key={source.id} value={source.id}>{source.name}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Category (Optional)</label>
            <select 
              className="flex h-10 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent shadow-sm"
              value={categoryId}
              onChange={(e) => setCategoryId(e.target.value)}
            >
              <option value="">None</option>
              {categories?.map(category => (
                <option key={category.id} value={category.id}>{category.name}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Frequency</label>
            <select 
              className="flex h-10 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent shadow-sm"
              value={frequencyId}
              onChange={(e) => setFrequencyId(e.target.value)}
              required
            >
              <option value="">Select Frequency</option>
              {frequencies?.map(freq => (
                <option key={freq.id} value={freq.id}>{freq.name}</option>
              ))}
            </select>
          </div>

          <div className="flex justify-end gap-3 pt-4">
            <Button 
              type="button" 
              variant="ghost" 
              onClick={() => setIsModalOpen(false)}
            >
              Cancel
            </Button>
            <Button 
              type="submit" 
              isLoading={isCreating}
              disabled={!name || !amount || !sourceId || !transactionTypeId || !frequencyId || !startDate}
            >
              Save Subscription
            </Button>
          </div>
        </form>
      </Modal>

      <Modal
        isOpen={isEditModalOpen}
        onClose={() => setIsEditModalOpen(false)}
        title="Edit Subscription"
      >
        <form onSubmit={handleUpdate} className="space-y-4">
          <Input
            label="Name"
            value={editName}
            onChange={(e) => setEditName(e.target.value)}
            required
            autoFocus
          />
          <Input
            label="Amount"
            type="number"
            step="0.01"
            value={editAmount}
            onChange={(e) => setEditAmount(e.target.value)}
            required
          />
          <Input
            label="Start Date"
            type="date"
            value={editStartDate}
            onChange={(e) => setEditStartDate(e.target.value)}
            required
          />

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Type</label>
            <select
              className="flex h-10 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent shadow-sm"
              value={editTransactionTypeId}
              onChange={(e) => setEditTransactionTypeId(e.target.value)}
              required
            >
              <option value="">Select Type</option>
              {transactionTypes?.map(type => (
                <option key={type.id} value={type.id}>{type.name}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Source (Wallet)</label>
            <select
              className="flex h-10 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent shadow-sm"
              value={editSourceId}
              onChange={(e) => setEditSourceId(e.target.value)}
              required
            >
              <option value="">Select Source</option>
              {sources?.map(source => (
                <option key={source.id} value={source.id}>{source.name}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Category (Optional)</label>
            <select
              className="flex h-10 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent shadow-sm"
              value={editCategoryId}
              onChange={(e) => setEditCategoryId(e.target.value)}
            >
              <option value="">None</option>
              {categories?.map(category => (
                <option key={category.id} value={category.id}>{category.name}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Frequency</label>
            <select
              className="flex h-10 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent shadow-sm"
              value={editFrequencyId}
              onChange={(e) => setEditFrequencyId(e.target.value)}
              required
            >
              <option value="">Select Frequency</option>
              {frequencies?.map(freq => (
                <option key={freq.id} value={freq.id}>{freq.name}</option>
              ))}
            </select>
          </div>

          <div className="flex justify-end gap-3 pt-4">
            <Button type="button" variant="ghost" onClick={() => setIsEditModalOpen(false)}>
              Cancel
            </Button>
            <Button
              type="submit"
              isLoading={isUpdating}
              disabled={!editName || !editAmount || !editSourceId || !editTransactionTypeId || !editFrequencyId || !editStartDate}
            >
              Save
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
