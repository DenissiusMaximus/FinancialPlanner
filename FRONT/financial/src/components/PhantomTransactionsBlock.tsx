import React from 'react';
import { Plus, Ghost, Trash2, Edit2, Sparkles } from 'lucide-react';
import type { PhantomTransaction } from '../hooks/usePhantomTransactions';
import { formatCurrency } from '../utils/formatters';
import { Card } from './Card';
import { useCurrencyConvert } from '../hooks/useCurrencyConvert';

interface PhantomTransactionsBlockProps {
  phantoms: PhantomTransaction[];
  togglePhantom: (id: string) => void;
  deletePhantom: (id: string) => void;
  onAddClick: () => void;
  onEditClick: (phantom: PhantomTransaction) => void;
}

export const PhantomTransactionsBlock: React.FC<PhantomTransactionsBlockProps> = ({
  phantoms, togglePhantom, deletePhantom, onAddClick, onEditClick
}) => {
  const { selectedCurrencyName } = useCurrencyConvert();

  const totalImpact = phantoms
    .filter(p => p.isEnabled)
    .reduce((sum, p) => {
      const isExpense = p.transactionType?.name?.toLowerCase().includes('витр') || p.transactionType?.name?.toLowerCase().includes('exp');
      const amount = p.amount || 0;
      return sum + (isExpense ? -amount : amount);
    }, 0);

  const activeCount = phantoms.filter(p => p.isEnabled).length;

  return (
    <Card className="bg-gradient-to-br from-[#f8f9fa] to-white border border-dashed border-[#d1d1d6] p-6 relative overflow-hidden transition-all duration-300 shadow-sm">
      <div className="absolute top-0 right-0 p-8 opacity-[0.03] pointer-events-none">
        <Sparkles size={120} />
      </div>

      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 relative z-10 mb-6">
        <div className="flex items-center gap-3">
          <div className="bg-primary/10 text-primary p-2.5 rounded-xl">
            <Ghost size={24} />
          </div>
          <div>
            <h3 className="text-xl font-semibold text-ink">Фантомні транзакції</h3>
            <p className="text-sm text-[#7a7a7a]">Моделюйте ситуації без впливу на базу даних.</p>
          </div>
        </div>
      </div>

      <div className="relative z-10">
        {phantoms.length === 0 ? (
          <div className="bg-white rounded-2xl border border-hairline p-8 flex flex-col items-center justify-center text-center">
            <div className="bg-[#f5f5f7] p-4 rounded-full mb-3">
              <Sparkles size={24} className="text-[#a0a0a0]" />
            </div>
            <h5 className="font-semibold text-ink mb-1">Немає фантомних транзакцій</h5>
            <p className="text-sm text-[#7a7a7a] w-full max-w-lg mx-auto mb-6 text-center">
              Додайте фантомні витрати або доходи, щоб побачити, як вони вплинуть на ваші цілі.
            </p>
            <button
              onClick={onAddClick}
              className="flex items-center gap-2 text-base font-semibold text-white bg-primary px-6 py-3 rounded-xl hover:bg-primary/90 transition-all active:scale-[0.98] shadow-sm shadow-primary/20"
            >
              <Plus size={20} />
              Додати транзакцію
            </button>
          </div>
        ) : (
          <div className="space-y-4">
            {/* Summary Row */}
            <div className="flex flex-wrap items-center justify-between gap-4 bg-white p-4 rounded-xl border border-hairline">
              <div className="flex items-center gap-4">
                <div className="text-sm">
                  <span className="text-[#7a7a7a]">Активних: </span>
                  <span className="font-bold text-ink">{activeCount} з {phantoms.length}</span>
                </div>
                <div className="h-4 w-px bg-hairline hidden sm:block"></div>
                <div className="text-sm">
                  <span className="text-[#7a7a7a]">Сумарний вплив: </span>
                  <span className={`font-mono font-bold ${totalImpact > 0 ? 'text-green-600' : totalImpact < 0 ? 'text-red-500' : 'text-ink'}`}>
                    {totalImpact > 0 ? '+' : ''}{formatCurrency(totalImpact, 0)} {selectedCurrencyName}
                  </span>
                </div>
              </div>
              <button
                onClick={onAddClick}
                className="w-full sm:w-auto flex items-center justify-center gap-1.5 text-sm font-semibold text-white bg-primary px-4 py-2 rounded-xl hover:bg-primary/90 transition-all active:scale-[0.98] shadow-sm shadow-primary/20"
              >
                <Plus size={18} />
                Додати ще
              </button>
            </div>

            {/* List */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {phantoms.map((p) => {
                const isExpense = p.transactionType?.name?.toLowerCase().includes('витр') || p.transactionType?.name?.toLowerCase().includes('exp');
                const sign = isExpense ? '-' : '+';
                const color = isExpense ? 'text-red-500' : 'text-green-600';
                
                return (
                  <div 
                    key={p.phantomId} 
                    className={`bg-white rounded-2xl border ${p.isEnabled ? 'border-primary/30 shadow-sm' : 'border-[#e5e5ea] opacity-60 grayscale-[0.2]'} p-4 transition-all duration-300`}
                  >
                    <div className="flex justify-between items-start gap-3">
                      <div className="flex items-start gap-3 flex-1 min-w-0">
                        <div className={`mt-1 w-2.5 h-2.5 rounded-full shrink-0 ${p.isEnabled ? (isExpense ? 'bg-red-500' : 'bg-green-500') : 'bg-[#d1d1d6]'}`} />
                        <div className="flex-1 min-w-0">
                          <h5 className={`font-semibold truncate text-lg ${p.isEnabled ? 'text-ink' : 'text-[#7a7a7a]'}`}>{p.name}</h5>
                          <div className="text-sm text-[#7a7a7a] mt-0.5 flex items-center gap-2 flex-wrap">
                            <span className="bg-[#f5f5f7] px-2 py-0.5 rounded-md font-medium">
                              {p.frequency?.name || 'Одноразово'}
                            </span>
                            <span className="text-[#d1d1d6]">•</span>
                            <span>{p.startDate ? new Date(p.startDate).toLocaleDateString('uk-UA') : 'Сьогодні'}</span>
                          </div>
                        </div>
                      </div>
                      
                      <button 
                        onClick={() => togglePhantom(p.phantomId)}
                        className={`relative inline-flex h-8 w-14 shrink-0 items-center rounded-full transition-colors duration-300 focus:outline-none ${p.isEnabled ? 'bg-primary' : 'bg-[#d1d1d6]'}`}
                        aria-pressed={p.isEnabled}
                      >
                        <span className="sr-only">Вимкнути/Увімкнути</span>
                        <span
                          className={`inline-block h-6 w-6 transform rounded-full bg-white transition-transform duration-300 ${p.isEnabled ? 'translate-x-7 shadow-sm' : 'translate-x-1'}`}
                        />
                      </button>
                    </div>
                    
                    <div className="flex items-end justify-between mt-5 pt-4 border-t border-hairline/50">
                      <div className={`font-mono font-bold text-xl ${p.isEnabled ? color : 'text-[#a0a0a0]'}`}>
                        {sign}{formatCurrency(p.amount || 0, 0)} <span className="text-sm font-sans font-medium">{typeof p.currency === 'string' ? p.currency : (p.currency?.name || selectedCurrencyName)}</span>
                      </div>
                      <div className="flex items-center gap-2">
                        <button 
                          onClick={() => onEditClick(p)}
                          className="p-2.5 text-[#7a7a7a] hover:text-primary hover:bg-primary/10 rounded-xl transition-colors"
                          aria-label="Редагувати"
                        >
                          <Edit2 size={18} />
                        </button>
                        <button 
                          onClick={() => deletePhantom(p.phantomId)}
                          className="p-2.5 text-[#7a7a7a] hover:text-red-500 hover:bg-red-50 rounded-xl transition-colors"
                          aria-label="Видалити"
                        >
                          <Trash2 size={18} />
                        </button>
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        )}
      </div>
    </Card>
  );
};
