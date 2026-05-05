import React, { useState } from 'react';
import {
  useGetSourcesSummaryQuery,
  useCreateSourceMutation,
  useUpdateSourceMutation,
  useDeleteSourceMutation,
  useArchiveSourceMutation,
  useUnarchiveSourceMutation,
} from '../../store/apiSlice';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/Card';
import { Wallet, Plus, Pencil, Trash2, ArchiveRestore, Archive } from 'lucide-react';
import { Button } from '../../components/ui/Button';
import { Modal } from '../../components/ui/Modal';
import { Input } from '../../components/ui/Input';
import toast from 'react-hot-toast';
import { formatCurrency } from '../../utils/number';

type SourceItem = {
  id: number;
  name?: string | null;
  amount?: number;
  isArchived?: boolean;
};

export function Sources() {
  const { data: sourceSummary, isLoading, error } = useGetSourcesSummaryQuery();
  const [createSource, { isLoading: isCreating }] = useCreateSourceMutation();
  const [updateSource, { isLoading: isUpdating }] = useUpdateSourceMutation();
  const [deleteSource] = useDeleteSourceMutation();
  const [archiveSource] = useArchiveSourceMutation();
  const [unarchiveSource] = useUnarchiveSourceMutation();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [newSourceName, setNewSourceName] = useState('');
  const [newSourceAmount, setNewSourceAmount] = useState('0');
  const [editName, setEditName] = useState('');
  const [editingSourceId, setEditingSourceId] = useState<number | null>(null);

  // /api/Source/summary returns { total, sources[] } — not a plain array
  const sources = (sourceSummary?.sources ?? []) as SourceItem[];

  const handleCreateSource = async (e) => {
    e.preventDefault();
    if (!newSourceName.trim()) return;

    try {
      await createSource({
        name: newSourceName.trim(),
        amount: parseFloat(newSourceAmount) || 0,
        currencyId: 1,
      }).unwrap();
      toast.success('Source created successfully');
      setNewSourceName('');
      setNewSourceAmount('0');
      setIsModalOpen(false);
    } catch (err) {
      toast.error('Failed to create source');
      console.error(err);
    }
  };

  const openEditModal = (source: SourceItem) => {
    setEditingSourceId(source.id);
    setEditName(source.name ?? '');
    setIsEditModalOpen(true);
  };

  const handleUpdateSource = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!editingSourceId || !editName.trim()) return;

    try {
      await updateSource({ id: editingSourceId, name: editName.trim() }).unwrap();
      toast.success('Source updated');
      setIsEditModalOpen(false);
      setEditingSourceId(null);
      setEditName('');
    } catch (err) {
      toast.error('Failed to update source');
      console.error(err);
    }
  };

  const handleDeleteSource = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this source?')) return;

    try {
      await deleteSource(id).unwrap();
      toast.success('Source deleted');
    } catch (err) {
      toast.error('Failed to delete source');
      console.error(err);
    }
  };

  const handleToggleArchive = async (source: SourceItem) => {
    try {
      if (source.isArchived) {
        await unarchiveSource(source.id).unwrap();
        toast.success('Source unarchived');
      } else {
        await archiveSource(source.id).unwrap();
        toast.success('Source archived');
      }
    } catch (err) {
      toast.error('Failed to change archive state');
      console.error(err);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">Sources</h1>
          <p className="text-sm text-slate-500 mt-1">Manage your accounts and wallets.</p>
        </div>
        <div className="mt-4 sm:mt-0">
          <Button onClick={() => setIsModalOpen(true)} className="flex items-center gap-2">
            <Plus className="h-4 w-4" /> Add Source
          </Button>
        </div>
      </div>

      {isLoading ? (
        <div className="text-center py-12 text-slate-500">Loading sources...</div>
      ) : error ? (
        <div className="text-center py-12 text-red-500">Failed to load sources.</div>
      ) : sources?.length > 0 ? (
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {sources.map((source) => (
            <Card key={source.id} className="hover:shadow-md transition-shadow">
              <CardHeader className="flex flex-row items-center justify-between pb-2">
                <div className="flex items-center space-x-3">
                  <div className="p-2 bg-primary-50 rounded-lg">
                    <Wallet className="h-5 w-5 text-primary-600" />
                  </div>
                  <CardTitle className="text-base font-medium">{source.name}</CardTitle>
                </div>
                <div className="flex items-center">
                  <button
                    onClick={() => openEditModal(source)}
                    className="text-slate-400 hover:text-primary-600 transition-colors p-2 rounded-md hover:bg-primary-50"
                    title="Edit Source"
                  >
                    <Pencil className="h-4 w-4" />
                  </button>
                  <button
                    onClick={() => handleToggleArchive(source)}
                    className="text-slate-400 hover:text-amber-600 transition-colors p-2 rounded-md hover:bg-amber-50"
                    title={source.isArchived ? 'Unarchive Source' : 'Archive Source'}
                  >
                    {source.isArchived ? <ArchiveRestore className="h-4 w-4" /> : <Archive className="h-4 w-4" />}
                  </button>
                  <button
                    onClick={() => handleDeleteSource(source.id)}
                    className="text-slate-400 hover:text-red-600 transition-colors p-2 rounded-md hover:bg-red-50"
                    title="Delete Source"
                  >
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </CardHeader>
              <CardContent>
                <div className="mt-4">
                  <span className="text-2xl font-bold text-slate-900">
                    ${formatCurrency(source.amount, { minimumFractionDigits: 2 })}
                  </span>
                </div>
                {source.isArchived && (
                  <p className="mt-2 text-xs text-amber-600 font-medium">Archived</p>
                )}
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <div className="text-center py-16 bg-white border border-dashed border-slate-300 rounded-xl">
          <div className="inline-flex h-12 w-12 items-center justify-center rounded-full bg-slate-50 mb-4">
            <Wallet className="h-6 w-6 text-slate-400" />
          </div>
          <h3 className="text-base font-semibold text-slate-900">No sources found</h3>
          <p className="text-sm text-slate-500 mt-1 max-w-sm mx-auto">
            You don't have any accounts or wallets yet. Add one to start tracking your finances.
          </p>
          <div className="mt-6">
            <Button onClick={() => setIsModalOpen(true)}>Add your first source</Button>
          </div>
        </div>
      )}

      <Modal 
        isOpen={isModalOpen} 
        onClose={() => setIsModalOpen(false)} 
        title="Add New Source"
      >
        <form onSubmit={handleCreateSource} className="space-y-4">
          <Input
            label="Source Name"
            placeholder="e.g. Main Bank Account"
            value={newSourceName}
            onChange={(e) => setNewSourceName(e.target.value)}
            required
            autoFocus
          />
          <Input
            label="Initial Amount"
            type="number"
            step="0.01"
            placeholder="0.00"
            value={newSourceAmount}
            onChange={(e) => setNewSourceAmount(e.target.value)}
            required
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
              disabled={!newSourceName.trim()}
            >
              Create
            </Button>
          </div>
        </form>
      </Modal>

      <Modal
        isOpen={isEditModalOpen}
        onClose={() => setIsEditModalOpen(false)}
        title="Edit Source"
      >
        <form onSubmit={handleUpdateSource} className="space-y-4">
          <Input
            label="Source Name"
            value={editName}
            onChange={(e) => setEditName(e.target.value)}
            required
            autoFocus
          />
          <div className="flex justify-end gap-3 pt-4">
            <Button type="button" variant="ghost" onClick={() => setIsEditModalOpen(false)}>
              Cancel
            </Button>
            <Button type="submit" isLoading={isUpdating} disabled={!editName.trim()}>
              Save
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
