import React, { useState } from 'react';
import { 
  useGetAimsQuery, 
  useCreateAimMutation,
  useUpdateAimMutation,
  useDeleteAimMutation
} from '../../store/apiSlice';
import { Card, CardContent, CardHeader, CardTitle } from '../../components/ui/Card';
import { Target, Plus, Trash2, Pencil } from 'lucide-react';
import { Button } from '../../components/ui/Button';
import { Modal } from '../../components/ui/Modal';
import { Input } from '../../components/ui/Input';
import toast from 'react-hot-toast';
import { formatCurrency, toSafeNumber } from '../../utils/number';

export function Aims() {
  const { data: aims, isLoading, error } = useGetAimsQuery({});
  const [createAim, { isLoading: isCreating }] = useCreateAimMutation();
  const [updateAim, { isLoading: isUpdating }] = useUpdateAimMutation();
  const [deleteAim] = useDeleteAimMutation();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [name, setName] = useState('');
  const [amount, setAmount] = useState('');
  const [editName, setEditName] = useState('');
  const [editAmount, setEditAmount] = useState('');
  const [editingAimId, setEditingAimId] = useState<number | null>(null);

  const handleCreateAim = async (e) => {
    e.preventDefault();
    if (!name.trim() || !amount) return;

    try {
      await createAim({ 
        name: name.trim(), 
        amount: parseFloat(amount),
        priority: 1, // default priority
        currencyId: 1 // default currency
      }).unwrap();
      
      toast.success('Aim created successfully');
      setName('');
      setAmount('');
      setIsModalOpen(false);
    } catch (err) {
      toast.error('Failed to create aim');
      console.error(err);
    }
  };

  const handleDeleteAim = async (id) => {
    if (!window.confirm('Are you sure you want to delete this aim?')) return;
    
    try {
      await deleteAim(id).unwrap();
      toast.success('Aim deleted');
    } catch (err) {
      toast.error('Failed to delete aim');
      console.error(err);
    }
  };

  const openEditAim = (aim) => {
    setEditingAimId(aim.id);
    setEditName(aim.name ?? '');
    setEditAmount(String(toSafeNumber(aim.amount)));
    setIsEditModalOpen(true);
  };

  const handleUpdateAim = async (e) => {
    e.preventDefault();
    if (!editingAimId || !editName.trim() || !editAmount) return;

    try {
      await updateAim({
        id: editingAimId,
        name: editName.trim(),
        amount: parseFloat(editAmount),
      }).unwrap();

      toast.success('Aim updated');
      setIsEditModalOpen(false);
      setEditingAimId(null);
      setEditName('');
      setEditAmount('');
    } catch (err) {
      toast.error('Failed to update aim');
      console.error(err);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">Aims</h1>
          <p className="text-sm text-slate-500 mt-1">Set and track your financial goals.</p>
        </div>
        <div className="mt-4 sm:mt-0">
          <Button onClick={() => setIsModalOpen(true)} className="flex items-center gap-2">
            <Plus className="h-4 w-4" /> Add Aim
          </Button>
        </div>
      </div>

      {isLoading ? (
        <div className="text-center py-12 text-slate-500">Loading aims...</div>
      ) : error ? (
        <div className="text-center py-12 text-red-500">Failed to load aims.</div>
      ) : aims?.length > 0 ? (
        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
          {aims.map((aim) => {
            const currentAmount = toSafeNumber(aim.progress?.collectedAmount);
            const targetAmount = toSafeNumber(aim.amount);
            const progressPercent = targetAmount > 0 
              ? Math.min(Math.round((currentAmount / targetAmount) * 100), 100) 
              : 0;

            return (
              <Card key={aim.id} className="hover:shadow-md transition-shadow flex flex-col">
                <CardHeader className="flex flex-row items-start justify-between pb-2 space-y-0 relative">
                  <div className="flex items-center space-x-3">
                    <div className="p-2 bg-indigo-50 rounded-lg">
                      <Target className="h-5 w-5 text-indigo-600" />
                    </div>
                    <CardTitle className="text-base font-medium pr-8">{aim.name}</CardTitle>
                  </div>
                  <button 
                    onClick={() => openEditAim(aim)}
                    className="absolute top-4 right-12 text-slate-400 hover:text-primary-600 transition-colors p-1 rounded-md hover:bg-primary-50"
                  >
                    <Pencil className="h-4 w-4" />
                  </button>
                  <button 
                    onClick={() => handleDeleteAim(aim.id)}
                    className="absolute top-4 right-4 text-slate-400 hover:text-red-600 transition-colors p-1 rounded-md hover:bg-red-50"
                  >
                    <Trash2 className="h-4 w-4" />
                  </button>
                </CardHeader>
                <CardContent className="flex-1 flex flex-col justify-end">
                  <div className="mt-4 space-y-2">
                    <div className="flex justify-between text-sm">
                      <span className="font-medium text-slate-900">${formatCurrency(currentAmount, { maximumFractionDigits: 0 })}</span>
                      <span className="text-slate-500">of ${formatCurrency(targetAmount, { maximumFractionDigits: 0 })}</span>
                    </div>
                    
                    <div className="h-2 w-full bg-slate-100 rounded-full overflow-hidden">
                      <div 
                        className={`h-full rounded-full transition-all duration-500 ${progressPercent >= 100 ? 'bg-emerald-500' : 'bg-indigo-500'}`}
                        style={{ width: `${progressPercent}%` }}
                      ></div>
                    </div>
                    
                    <div className="flex justify-between text-xs text-slate-500 pt-1">
                      <span>{progressPercent}% Complete</span>
                      {aim.isClosed && <span className="text-emerald-600 font-medium">Goal Reached!</span>}
                    </div>
                  </div>
                </CardContent>
              </Card>
            );
          })}
        </div>
      ) : (
        <div className="text-center py-16 bg-white border border-dashed border-slate-300 rounded-xl">
          <div className="inline-flex h-12 w-12 items-center justify-center rounded-full bg-slate-50 mb-4">
            <Target className="h-6 w-6 text-slate-400" />
          </div>
          <h3 className="text-base font-semibold text-slate-900">No aims found</h3>
          <p className="text-sm text-slate-500 mt-1 max-w-sm mx-auto">
            You don't have any financial goals set. Create an aim to start saving for your future.
          </p>
          <div className="mt-6">
            <Button onClick={() => setIsModalOpen(true)}>Create your first aim</Button>
          </div>
        </div>
      )}

      <Modal 
        isOpen={isModalOpen} 
        onClose={() => setIsModalOpen(false)} 
        title="Add New Aim"
      >
        <form onSubmit={handleCreateAim} className="space-y-4">
          <Input
            label="Goal Name"
            placeholder="e.g. New Car, Vacation"
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
            autoFocus
          />
          <Input
            label="Target Amount"
            type="number"
            step="0.01"
            placeholder="5000.00"
            value={amount}
            onChange={(e) => setAmount(e.target.value)}
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
              disabled={!name.trim() || !amount}
            >
              Save Goal
            </Button>
          </div>
        </form>
      </Modal>

      <Modal
        isOpen={isEditModalOpen}
        onClose={() => setIsEditModalOpen(false)}
        title="Edit Aim"
      >
        <form onSubmit={handleUpdateAim} className="space-y-4">
          <Input
            label="Goal Name"
            value={editName}
            onChange={(e) => setEditName(e.target.value)}
            required
            autoFocus
          />
          <Input
            label="Target Amount"
            type="number"
            step="0.01"
            value={editAmount}
            onChange={(e) => setEditAmount(e.target.value)}
            required
          />
          <div className="flex justify-end gap-3 pt-4">
            <Button type="button" variant="ghost" onClick={() => setIsEditModalOpen(false)}>
              Cancel
            </Button>
            <Button type="submit" isLoading={isUpdating} disabled={!editName.trim() || !editAmount}>
              Save
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
