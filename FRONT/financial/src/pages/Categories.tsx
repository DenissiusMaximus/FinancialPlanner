import React, { useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { Button } from '../components/Button';
import { Modal } from '../components/Modal';
import { Skeleton } from '../components/Skeleton';
import { EmptyState } from '../components/EmptyState';
import { Pencil, Trash2, FolderOpen } from 'lucide-react';
import {
  useGetApiCategory,
  usePostApiCategory,
  usePatchApiCategoryId,
  useDeleteApiCategoryId,
} from '../api/generated/endpoints';

export const Categories: React.FC = () => {
  const queryClient = useQueryClient();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingCategory, setEditingCategory] = useState<{ id: number; name: string } | null>(null);
  const [categoryName, setCategoryName] = useState('');
  const [error, setError] = useState('');

  const categoriesQuery = useGetApiCategory();
  const createMutation = usePostApiCategory();
  const updateMutation = usePatchApiCategoryId();
  const deleteMutation = useDeleteApiCategoryId();

  const categories = Array.isArray(categoriesQuery.data) ? categoriesQuery.data : [];
  const isLoading = categoriesQuery.isLoading;

  const handleOpenCreate = () => {
    setEditingCategory(null);
    setCategoryName('');
    setError('');
    setIsModalOpen(true);
  };

  const handleOpenEdit = (category: any) => {
    setEditingCategory({ id: category.id, name: category.name });
    setCategoryName(category.name);
    setError('');
    setIsModalOpen(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!categoryName.trim()) {
      setError('Назва категорії не може бути порожньою');
      return;
    }

    try {
      if (editingCategory) {
        await updateMutation.mutateAsync({
          id: editingCategory.id,
          data: { name: categoryName.trim() },
        });
      } else {
        await createMutation.mutateAsync({
          data: { name: categoryName.trim() },
        });
      }
      setIsModalOpen(false);
      queryClient.invalidateQueries({ queryKey: ['/api/Category'] });
    } catch (err: any) {
      console.error('Error saving category:', err);
      if (err?.response?.data?.errors) {
        setError(JSON.stringify(err.response.data.errors));
      } else {
        setError('Помилка збереження категорії');
      }
    }
  };

  const handleDelete = async (id: number) => {
    if (confirm('Ви впевнені, що хочете видалити цю категорію?')) {
      try {
        await deleteMutation.mutateAsync({ id });
        queryClient.invalidateQueries({ queryKey: ['/api/Category'] });
      } catch (err) {
        console.error('Error deleting category:', err);
        alert('Помилка видалення категорії. Можливо вона використовується в транзакціях.');
      }
    }
  };

  if (isLoading) {
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
    <div className="w-full">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-2xl font-bold text-ink">Категорії</h1>
          <p className="text-sm text-[#7a7a7a] mt-1">Управляйте категоріями для ваших транзакцій</p>
        </div>
        <Button onClick={handleOpenCreate}>+ Нова категорія</Button>
      </div>

      {categories.length > 0 ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
          {categories.map((category: any) => (
            <div
              key={category.id}
              className="bg-white border border-hairline rounded-2xl p-5 shadow-sm hover:shadow-md transition-shadow group relative overflow-hidden"
            >
              <div className="flex items-center gap-3 mb-3">
                <div className="bg-primary/10 text-primary p-2.5 rounded-xl shrink-0">
                  <FolderOpen size={20} />
                </div>
                <h3 className="font-semibold text-ink text-base truncate" title={category.name}>
                  {category.name}
                </h3>
              </div>

              <div className="flex gap-2 justify-end mt-4 pt-4 border-t border-hairline">
                <button
                  onClick={() => handleOpenEdit(category)}
                  className="text-[#7a7a7a] hover:text-primary transition-colors p-1.5 rounded-lg hover:bg-primary/5 flex items-center justify-center"
                  title="Редагувати"
                >
                  <Pencil size={16} />
                </button>
                <button
                  onClick={() => handleDelete(category.id)}
                  className="text-[#7a7a7a] hover:text-red-500 transition-colors p-1.5 rounded-lg hover:bg-red-50 flex items-center justify-center"
                  title="Видалити"
                >
                  <Trash2 size={16} />
                </button>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <EmptyState
          title="Немає категорій"
          description="Створіть свою першу категорію для кращої аналітики."
          action={<Button onClick={handleOpenCreate}>+ Нова категорія</Button>}
        />
      )}

      {/* Modal for Create/Edit */}
      <Modal
        isOpen={isModalOpen}
        title={editingCategory ? "Редагувати категорію" : "Нова категорія"}
        onClose={() => setIsModalOpen(false)}
        size="sm"
      >
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-semibold text-ink mb-2">Назва категорії</label>
            <input
              type="text"
              value={categoryName}
              onChange={(e) => {
                setCategoryName(e.target.value);
                setError('');
              }}
              placeholder="Наприклад: Їжа, Транспорт..."
              className="w-full px-4 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-[#1d1d1f]"
              autoFocus
            />
            {error && <p className="mt-1 text-xs text-red-500">{error}</p>}
          </div>

          <div className="flex gap-3 pt-4">
            <Button variant="secondary" onClick={() => setIsModalOpen(false)} type="button">
              Скасувати
            </Button>
            <Button
              type="submit"
              isLoading={createMutation.isPending || updateMutation.isPending}
            >
              {editingCategory ? "Зберегти" : "Створити"}
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
};
