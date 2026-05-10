import React from 'react';
import { Calendar, Filter, X, ArrowDownWideNarrow, ArrowUpNarrowWide } from 'lucide-react';
import { useGetApiCategory, useGetApiTransactionType } from '../api/generated/endpoints';
import { getTransactionTypeLabel } from '../utils/display-helpers';

export interface TransactionFilters {
  FromDate?: string;
  ToDate?: string;
  CategoryId?: number;
  TransactionTypeId?: number;
  SortBy?: 'Date' | 'Amount';
  SortDescending?: boolean;
}

interface TransactionFilterProps {
  filters: TransactionFilters;
  onFilterChange: (filters: TransactionFilters) => void;
  onClearFilters: () => void;
}

export const TransactionFilter: React.FC<TransactionFilterProps> = ({
  filters,
  onFilterChange,
  onClearFilters,
}) => {
  const categoriesQuery = useGetApiCategory();
  const typesQuery = useGetApiTransactionType();

  const categories = Array.isArray(categoriesQuery.data) ? categoriesQuery.data : [];
  const types = Array.isArray(typesQuery.data) ? typesQuery.data : [];

  const hasActiveFilters = filters.FromDate || filters.ToDate || filters.CategoryId || filters.TransactionTypeId;

  const handleChange = (key: keyof TransactionFilters, value: any) => {
    onFilterChange({
      ...filters,
      [key]: value === '' ? undefined : value,
    });
  };

  const handleSortToggle = () => {
    onFilterChange({
      ...filters,
      SortDescending: !(filters.SortDescending ?? true),
    });
  };

  return (
    <div className="bg-white p-4 rounded-xl border border-hairline shadow-sm mb-6">
      <div className="flex items-center gap-2 mb-4 text-ink">
        <Filter size={18} />
        <h3 className="font-semibold text-sm">Фільтри та сортування</h3>
        {hasActiveFilters && (
          <button 
            onClick={onClearFilters}
            className="ml-auto text-xs text-[#7a7a7a] hover:text-red-500 flex items-center gap-1 transition-colors"
          >
            <X size={14} /> Очистити
          </button>
        )}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
        {/* Date From */}
        <div>
          <label className="block text-xs font-semibold text-[#7a7a7a] mb-1.5 flex items-center gap-1">
            <Calendar size={12} /> З дати
          </label>
          <input
            type="date"
            value={filters.FromDate || ''}
            onChange={(e) => handleChange('FromDate', e.target.value)}
            className="w-full px-3 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-sm text-[#1d1d1f]"
          />
        </div>

        {/* Date To */}
        <div>
          <label className="block text-xs font-semibold text-[#7a7a7a] mb-1.5 flex items-center gap-1">
            <Calendar size={12} /> По дату
          </label>
          <input
            type="date"
            value={filters.ToDate || ''}
            onChange={(e) => handleChange('ToDate', e.target.value)}
            className="w-full px-3 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-sm text-[#1d1d1f]"
          />
        </div>

        {/* Category */}
        <div>
          <label className="block text-xs font-semibold text-[#7a7a7a] mb-1.5">
            Категорія
          </label>
          <select
            value={filters.CategoryId || ''}
            onChange={(e) => handleChange('CategoryId', e.target.value ? Number(e.target.value) : undefined)}
            className="w-full px-3 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-sm text-[#1d1d1f]"
          >
            <option value="">Всі категорії</option>
            {categories.map((c: any) => (
              <option key={c.id} value={c.id}>{c.name}</option>
            ))}
          </select>
        </div>

        {/* Transaction Type */}
        <div>
          <label className="block text-xs font-semibold text-[#7a7a7a] mb-1.5">
            Тип транзакції
          </label>
          <select
            value={filters.TransactionTypeId || ''}
            onChange={(e) => handleChange('TransactionTypeId', e.target.value ? Number(e.target.value) : undefined)}
            className="w-full px-3 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-sm text-[#1d1d1f]"
          >
            <option value="">Всі типи</option>
            {types.map((t: any) => (
              <option key={t.id} value={t.id}>{getTransactionTypeLabel(t.name).label}</option>
            ))}
          </select>
        </div>

        {/* Sort */}
        <div>
          <label className="block text-xs font-semibold text-[#7a7a7a] mb-1.5">
            Сортування
          </label>
          <div className="flex gap-2">
            <select
              value={filters.SortBy || 'Date'}
              onChange={(e) => handleChange('SortBy', e.target.value)}
              className="w-full px-3 py-2 border border-hairline rounded-lg focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary/20 text-sm text-[#1d1d1f]"
            >
              <option value="Date">За датою</option>
              <option value="Amount">За сумою</option>
            </select>
            <button
              onClick={handleSortToggle}
              className="p-2 border border-hairline rounded-lg hover:bg-gray-50 transition-colors text-ink bg-white flex items-center justify-center shrink-0 w-[42px]"
              title={filters.SortDescending ?? true ? "Спадання (нові спочатку)" : "Зростання (старі спочатку)"}
            >
              {filters.SortDescending ?? true ? <ArrowDownWideNarrow size={18} /> : <ArrowUpNarrowWide size={18} />}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
