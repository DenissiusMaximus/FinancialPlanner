import React, { useState } from 'react';
import { 
  useGetTransactionsQuery, 
  useCreateTransactionMutation,
  useUpdateTransactionMutation,
  useDeleteTransactionMutation,
  useGetSourcesSummaryQuery,
  useGetCategoriesQuery,
  useGetTransactionTypesQuery
} from '../../store/apiSlice';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../../components/ui/Card';
import { TrendingDown, TrendingUp, Search, Plus, Pencil, Trash2 } from 'lucide-react';
import { Input } from '../../components/ui/Input';
import { Button } from '../../components/ui/Button';
import { Modal } from '../../components/ui/Modal';
import toast from 'react-hot-toast';
import { formatCurrency, toSafeNumber } from '../../utils/number';

type Lookup = { id: number; name?: string | null; amount?: number };
type TransactionItem = {
  id: number;
  amount?: number;
  comment?: string | null;
  date: string;
  category?: { id: number; name?: string | null } | null;
  source?: { id: number; name?: string | null } | null;
  transactionType?: { id: number; name?: string | null } | null;
};

export function Transactions() {
  const [searchTerm, setSearchTerm] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  
  // Queries
  const { data: transactionsResult, isLoading, error } = useGetTransactionsQuery({ limit: 50, offset: 0 });
  const { data: sourceSummary } = useGetSourcesSummaryQuery();
  const { data: categories } = useGetCategoriesQuery();
  const { data: transactionTypes } = useGetTransactionTypesQuery();
  
  const [createTransaction, { isLoading: isCreating }] = useCreateTransactionMutation();
  const [updateTransaction, { isLoading: isUpdating }] = useUpdateTransactionMutation();
  const [deleteTransaction] = useDeleteTransactionMutation();

  // /api/Transaction returns { data[], meta } paginated
  const transactions = (transactionsResult?.data ?? []) as TransactionItem[];
  // /api/Source/summary returns { total, sources[] }
  const sources = (sourceSummary?.sources ?? []) as Lookup[];

  const filteredTransactions = transactions.filter(tx =>
    tx.comment?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  // Form State
  const [amount, setAmount] = useState('');
  const [comment, setComment] = useState('');
  const [sourceId, setSourceId] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [transactionTypeId, setTransactionTypeId] = useState('');
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [editAmount, setEditAmount] = useState('');
  const [editComment, setEditComment] = useState('');
  const [editDate, setEditDate] = useState(new Date().toISOString().split('T')[0]);
  const [editSourceId, setEditSourceId] = useState('');
  const [editCategoryId, setEditCategoryId] = useState('');
  const [editTransactionTypeId, setEditTransactionTypeId] = useState('');

  const handleCreateTransaction = async (e) => {
    e.preventDefault();
    if (!amount || !sourceId || !categoryId || !transactionTypeId) return;

    try {
      await createTransaction({
        amount: parseFloat(amount),
        comment,
        sourceId: parseInt(sourceId),
        categoryId: parseInt(categoryId),
        transactionTypeId: parseInt(transactionTypeId),
        currencyId: 1,
        date: new Date().toISOString().split('T')[0],
      }).unwrap();
      
      toast.success('Transaction added successfully');
      setAmount('');
      setComment('');
      setSourceId('');
      setCategoryId('');
      setTransactionTypeId('');
      setIsModalOpen(false);
    } catch (err) {
      toast.error('Failed to add transaction');
      console.error(err);
    }
  };

  const openEditModal = (tx: TransactionItem) => {
    setEditingId(tx.id);
    setEditAmount(String(toSafeNumber(tx.amount)));
    setEditComment(tx.comment ?? '');
    setEditDate((tx.date ?? new Date().toISOString()).split('T')[0]);
    setEditSourceId(tx.source?.id ? String(tx.source.id) : '');
    setEditCategoryId(tx.category?.id ? String(tx.category.id) : '');
    setEditTransactionTypeId(tx.transactionType?.id ? String(tx.transactionType.id) : '');
    setIsEditModalOpen(true);
  };

  const handleUpdateTransaction = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!editingId || !editAmount || !editSourceId || !editTransactionTypeId || !editDate) return;

    try {
      await updateTransaction({
        id: editingId,
        amount: parseFloat(editAmount),
        comment: editComment,
        date: editDate,
        sourceId: parseInt(editSourceId),
        categoryId: editCategoryId ? parseInt(editCategoryId) : null,
        transactionTypeId: parseInt(editTransactionTypeId),
        currencyId: 1,
      }).unwrap();

      toast.success('Transaction updated');
      setIsEditModalOpen(false);
      setEditingId(null);
    } catch (err) {
      toast.error('Failed to update transaction');
      console.error(err);
    }
  };

  const handleDeleteTransaction = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this transaction?')) return;

    try {
      await deleteTransaction(id).unwrap();
      toast.success('Transaction deleted');
    } catch (err) {
      toast.error('Failed to delete transaction');
      console.error(err);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center">
        <h1 className="text-3xl font-bold tracking-tight text-slate-900">Transactions</h1>
        <div className="mt-4 sm:mt-0">
          <Button onClick={() => setIsModalOpen(true)} className="flex items-center gap-2">
            <Plus className="h-4 w-4" /> Add Transaction
          </Button>
        </div>
      </div>

      <Card>
        <CardHeader className="border-b border-slate-100 pb-4">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
            <div>
              <CardTitle>History</CardTitle>
              <CardDescription>View and manage your transactions.</CardDescription>
            </div>
            <div className="flex items-center gap-2 w-full sm:w-auto">
              <div className="relative flex-1 sm:w-64">
                <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-slate-400" />
                <Input
                  type="search"
                  placeholder="Search transactions..."
                  className="pl-9"
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                />
              </div>
            </div>
          </div>
        </CardHeader>
        <CardContent className="p-0">
          {isLoading ? (
            <div className="text-center py-12 text-slate-500">Loading...</div>
          ) : error ? (
            <div className="text-center py-12 text-red-500">Failed to load transactions.</div>
          ) : filteredTransactions?.length > 0 ? (
            <div className="divide-y divide-slate-100">
              {filteredTransactions.map((tx) => {
                const amountValue = toSafeNumber(tx.amount);

                return (
                <div key={tx.id} className="flex items-center justify-between p-4 sm:px-6 hover:bg-slate-50 transition-colors">
                  <div className="flex items-center space-x-4">
                    <div className={`p-2 rounded-full ${amountValue < 0 ? 'bg-red-100 text-red-600' : 'bg-emerald-100 text-emerald-600'}`}>
                      {amountValue < 0 ? <TrendingDown className="h-4 w-4" /> : <TrendingUp className="h-4 w-4" />}
                    </div>
                    <div>
                      <p className="text-sm font-medium text-slate-900">{tx.comment || 'Transaction'}</p>
                      <div className="flex items-center gap-2 text-xs text-slate-500">
                        <span>{new Date(tx.date).toLocaleDateString()}</span>
                      </div>
                    </div>
                  </div>
                  <div className={`font-semibold ${amountValue < 0 ? 'text-slate-900' : 'text-emerald-600'}`}>
                    {amountValue < 0 ? '-' : '+'}${formatCurrency(Math.abs(amountValue))}
                  </div>
                  <div className="flex items-center ml-4">
                    <button
                      onClick={() => openEditModal(tx)}
                      className="text-slate-400 hover:text-primary-600 transition-colors p-2 rounded-md hover:bg-primary-50"
                      title="Edit Transaction"
                    >
                      <Pencil className="h-4 w-4" />
                    </button>
                    <button
                      onClick={() => handleDeleteTransaction(tx.id)}
                      className="text-slate-400 hover:text-red-600 transition-colors p-2 rounded-md hover:bg-red-50"
                      title="Delete Transaction"
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
                <Search className="h-6 w-6 text-slate-400" />
              </div>
              <h3 className="text-sm font-medium text-slate-900">No transactions found</h3>
              <p className="text-sm text-slate-500 mt-1">Try adjusting your search query.</p>
            </div>
          )}
        </CardContent>
      </Card>

      <Modal 
        isOpen={isModalOpen} 
        onClose={() => setIsModalOpen(false)} 
        title="Add New Transaction"
      >
        <form onSubmit={handleCreateTransaction} className="space-y-4">
          <Input
            label="Amount"
            type="number"
            step="0.01"
            placeholder="0.00"
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
            required
            autoFocus
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
                <option key={source.id} value={source.id}>{source.name} (${formatCurrency(source.amount, { minimumFractionDigits: 2 })})</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-slate-700 mb-1">Category</label>
            <select 
              className="flex h-10 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent shadow-sm"
              value={categoryId}
              onChange={(e) => setCategoryId(e.target.value)}
              required
            >
              <option value="">Select Category</option>
              {categories?.map(category => (
                <option key={category.id} value={category.id}>{category.name}</option>
              ))}
            </select>
          </div>

          <Input
            label="Comment (Optional)"
            placeholder="What was this for?"
            value={comment}
            onChange={(e) => setComment(e.target.value)}
          />

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
              disabled={!amount || !sourceId || !categoryId || !transactionTypeId}
            >
              Save Transaction
            </Button>
          </div>
        </form>
      </Modal>

      <Modal
        isOpen={isEditModalOpen}
        onClose={() => setIsEditModalOpen(false)}
        title="Edit Transaction"
      >
        <form onSubmit={handleUpdateTransaction} className="space-y-4">
          <Input
            label="Amount"
            type="number"
            step="0.01"
            value={editAmount}
            onChange={(e) => setEditAmount(e.target.value)}
            required
            autoFocus
          />
          <Input
            label="Date"
            type="date"
            value={editDate}
            onChange={(e) => setEditDate(e.target.value)}
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

          <Input
            label="Comment (Optional)"
            value={editComment}
            onChange={(e) => setEditComment(e.target.value)}
          />

          <div className="flex justify-end gap-3 pt-4">
            <Button type="button" variant="ghost" onClick={() => setIsEditModalOpen(false)}>
              Cancel
            </Button>
            <Button
              type="submit"
              isLoading={isUpdating}
              disabled={!editAmount || !editSourceId || !editTransactionTypeId || !editDate}
            >
              Save
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
