import React, { useState } from 'react';
import { 
  useGetCategoriesQuery, 
  useCreateCategoryMutation,
  useUpdateCategoryMutation,
  useDeleteCategoryMutation
} from '../../store/apiSlice';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../../components/ui/Card';
import { Folder, Plus, Trash2, Pencil } from 'lucide-react';
import { Button } from '../../components/ui/Button';
import { Modal } from '../../components/ui/Modal';
import { Input } from '../../components/ui/Input';
import toast from 'react-hot-toast';

export function Categories() {
  const { data: categories, isLoading, error } = useGetCategoriesQuery();
  const [createCategory, { isLoading: isCreating }] = useCreateCategoryMutation();
  const [updateCategory, { isLoading: isUpdating }] = useUpdateCategoryMutation();
  const [deleteCategory] = useDeleteCategoryMutation();
  
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [newCategoryName, setNewCategoryName] = useState('');
  const [editCategoryName, setEditCategoryName] = useState('');
  const [editingCategoryId, setEditingCategoryId] = useState<number | null>(null);

  const handleCreateCategory = async (e) => {
    e.preventDefault();
    if (!newCategoryName.trim()) return;

    try {
      await createCategory({ name: newCategoryName.trim() }).unwrap();
      toast.success('Category created successfully');
      setNewCategoryName('');
      setIsModalOpen(false);
    } catch (err) {
      toast.error('Failed to create category');
      console.error(err);
    }
  };

  const handleDeleteCategory = async (id) => {
    if (!window.confirm('Are you sure you want to delete this category?')) return;
    
    try {
      await deleteCategory(id).unwrap();
      toast.success('Category deleted');
    } catch (err) {
      toast.error('Failed to delete category. It might be in use.');
      console.error(err);
    }
  };

  const openEditCategory = (category) => {
    setEditingCategoryId(category.id);
    setEditCategoryName(category.name ?? '');
    setIsEditModalOpen(true);
  };

  const handleUpdateCategory = async (e) => {
    e.preventDefault();
    if (!editingCategoryId || !editCategoryName.trim()) return;

    try {
      await updateCategory({ id: editingCategoryId, name: editCategoryName.trim() }).unwrap();
      toast.success('Category updated');
      setIsEditModalOpen(false);
      setEditingCategoryId(null);
      setEditCategoryName('');
    } catch (err) {
      toast.error('Failed to update category');
      console.error(err);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-slate-900">Categories</h1>
          <p className="text-sm text-slate-500 mt-1">Manage your transaction categories.</p>
        </div>
        <div className="mt-4 sm:mt-0">
          <Button onClick={() => setIsModalOpen(true)} className="flex items-center gap-2">
            <Plus className="h-4 w-4" /> Add Category
          </Button>
        </div>
      </div>

      <Card>
        <CardHeader className="border-b border-slate-100 pb-4">
          <CardTitle>All Categories</CardTitle>
          <CardDescription>Organize your transactions effectively.</CardDescription>
        </CardHeader>
        <CardContent className="p-0">
          {isLoading ? (
            <div className="text-center py-12 text-slate-500">Loading categories...</div>
          ) : error ? (
            <div className="text-center py-12 text-red-500">Failed to load categories.</div>
          ) : categories?.length > 0 ? (
            <div className="divide-y divide-slate-100">
              {categories.map((category) => (
                <div key={category.id} className="flex items-center justify-between p-4 sm:px-6 hover:bg-slate-50 transition-colors">
                  <div className="flex items-center space-x-4">
                    <div className="p-2 rounded-full bg-indigo-100 text-indigo-600">
                      <Folder className="h-4 w-4" />
                    </div>
                    <div>
                      <p className="text-sm font-medium text-slate-900">{category.name}</p>
                    </div>
                  </div>
                  <div>
                    <button
                      onClick={() => openEditCategory(category)}
                      className="text-slate-400 hover:text-primary-600 transition-colors p-2 rounded-md hover:bg-primary-50"
                      title="Edit Category"
                    >
                      <Pencil className="h-4 w-4" />
                    </button>
                    <button 
                      onClick={() => handleDeleteCategory(category.id)}
                      className="text-slate-400 hover:text-red-600 transition-colors p-2 rounded-md hover:bg-red-50"
                      title="Delete Category"
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <div className="text-center py-16">
              <div className="inline-flex h-12 w-12 items-center justify-center rounded-full bg-slate-100 mb-4">
                <Folder className="h-6 w-6 text-slate-400" />
              </div>
              <h3 className="text-sm font-medium text-slate-900">No categories found</h3>
              <p className="text-sm text-slate-500 mt-1">Create your first category to start organizing.</p>
            </div>
          )}
        </CardContent>
      </Card>

      <Modal 
        isOpen={isModalOpen} 
        onClose={() => setIsModalOpen(false)} 
        title="Add New Category"
      >
        <form onSubmit={handleCreateCategory} className="space-y-4">
          <Input
            label="Category Name"
            placeholder="e.g. Groceries"
            value={newCategoryName}
            onChange={(e) => setNewCategoryName(e.target.value)}
            required
            autoFocus
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
              disabled={!newCategoryName.trim()}
            >
              Create
            </Button>
          </div>
        </form>
      </Modal>

      <Modal
        isOpen={isEditModalOpen}
        onClose={() => setIsEditModalOpen(false)}
        title="Edit Category"
      >
        <form onSubmit={handleUpdateCategory} className="space-y-4">
          <Input
            label="Category Name"
            value={editCategoryName}
            onChange={(e) => setEditCategoryName(e.target.value)}
            required
            autoFocus
          />
          <div className="flex justify-end gap-3 pt-4">
            <Button type="button" variant="ghost" onClick={() => setIsEditModalOpen(false)}>
              Cancel
            </Button>
            <Button type="submit" isLoading={isUpdating} disabled={!editCategoryName.trim()}>
              Save
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
