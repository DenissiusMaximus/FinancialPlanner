import React, { useState } from 'react';
import { Plus, Ghost, Trash2, Edit2, Sparkles, ChevronDown, ChevronUp } from 'lucide-react';
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
  const [isExpanded, setIsExpanded] = useState(false);
  const { selectedCurrencyName } = useCurrencyConvert();

  return (
    <Card className="bg-gradient-to-br from-[#f8f9fa] to-white border border-dashed border-[#d1d1d6] p-6 relative overflow-hidden transition-all duration-300 shadow-sm hover:shadow-md">
      {/* Decorative background element */}
      <div className="absolute top-0 right-0 p-8 opacity-[0.03] pointer-events-none">
        <Sparkles size={120} />
      </div>

      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 relative z-10">
        <div>
          <div className="flex items-center gap-2 mb-1">
            <div className="bg-primary/10 text-primary p-2 rounded-xl">
              <Ghost size={20} />
            </div>
            <h3 className="text-xl font-semibold text-ink">Фантомні транзакції</h3>
            <span className="bg-primary text-white text-[10px] font-bold px-2 py-0.5 rounded-full uppercase tracking-wider">Beta</span>
          </div>
          <p className="text-sm text-[#7a7a7a]">
            Моделюйте ситуації (наприклад, "купівля джинсів") без впливу на реальну базу даних. Дані зникнуть завтра.
          </p>
        </div>

        <button 
          onClick={() => setIsExpanded(!isExpanded)}
          className="w-full sm:w-auto flex items-center justify-center gap-2 text-sm font-semibold text-ink bg-white border border-[#e5e5ea] px-4 py-2.5 rounded-xl hover:bg-[#f5f5f7] transition-all active:scale-[0.98]"
        >
          {isExpanded ? 'Приховати панель' : 'Відкрити панель'}
          {isExpanded ? <ChevronUp size={16} /> : <ChevronDown size={16} />}
        </button>
      </div>

      {isExpanded && (
        <div className="mt-6 pt-6 border-t border-[#e5e5ea] animate-in fade-in slide-in-from-top-4 duration-300 relative z-10">
          <div className="flex justify-between items-center mb-4">
            <h4 className="text-sm font-bold uppercase text-[#7a7a7a] tracking-wider">Ваші моделі</h4>
            <button
              onClick={onAddClick}
              className="flex items-center gap-1.5 text-sm font-semibold text-white bg-primary px-4 py-2 rounded-xl hover:bg-primary/90 transition-all active:scale-[0.98] shadow-sm shadow-primary/20"
            >
              <Plus size={16} />
              Додати
            </button>
          </div>

          {phantoms.length === 0 ? (
            <div className="bg-white rounded-2xl border border-hairline p-8 flex flex-col items-center justify-center text-center">
              <div className="bg-[#f5f5f7] p-4 rounded-full mb-3">
                <Sparkles size={24} className="text-[#a0a0a0]" />
              </div>
              <h5 className="font-semibold text-ink mb-1">Немає фантомних транзакцій</h5>
              <p className="text-sm text-[#7a7a7a] max-w-sm mx-auto">
                Додайте фантомні витрати або доходи, щоб побачити, як вони вплинуть на ваші цілі та накопичення.
              </p>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {phantoms.map((p) => {
                const isExpense = p.transactionType?.name?.toLowerCase().includes('витр') || p.transactionType?.name?.toLowerCase().includes('exp');
                const sign = isExpense ? '-' : '+';
                const color = isExpense ? 'text-red-500' : 'text-green-600';
                
                return (
                  <div 
                    key={p.phantomId} 
                    className={`bg-white rounded-2xl border ${p.isEnabled ? 'border-primary/30 shadow-sm' : 'border-[#e5e5ea] opacity-70'} p-4 transition-all duration-200`}
                  >
                    <div className="flex justify-between items-start mb-3">
                      <div>
                        <h5 className={`font-semibold ${p.isEnabled ? 'text-ink' : 'text-[#7a7a7a]'}`}>{p.name}</h5>
                        <div className="text-xs text-[#7a7a7a] mt-0.5 flex items-center gap-1.5 flex-wrap">
                          <span className="uppercase text-[10px] font-bold bg-[#f5f5f7] px-1.5 py-0.5 rounded-md">
                            {p.frequency?.name || 'Одноразово'}
                          </span>
                          <span>•</span>
                          <span>{p.startDate ? new Date(p.startDate).toLocaleDateString('uk-UA') : 'Сьогодні'}</span>
                        </div>
                      </div>
                      
                      {/* Custom Toggle Switch */}
                      <button 
                        onClick={() => togglePhantom(p.phantomId)}
                        className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors duration-300 focus:outline-none ${p.isEnabled ? 'bg-primary' : 'bg-[#d1d1d6]'}`}
                        aria-pressed={p.isEnabled}
                      >
                        <span className="sr-only">Вимкнути/Увімкнути</span>
                        <span
                          className={`inline-block h-5 w-5 transform rounded-full bg-white transition duration-300 ${p.isEnabled ? 'translate-x-5 shadow-sm' : 'translate-x-0.5'}`}
                        />
                      </button>
                    </div>
                    
                    <div className="flex items-end justify-between mt-4">
                      <div className={`font-mono font-bold text-lg ${p.isEnabled ? color : 'text-[#a0a0a0]'}`}>
                        {sign}{formatCurrency(p.amount || 0, 0)} {typeof p.currency === 'string' ? p.currency : (p.currency?.name || selectedCurrencyName)}
                      </div>
                      <div className="flex items-center gap-1">
                        <button 
                          onClick={() => onEditClick(p)}
                          className="p-2 text-[#7a7a7a] hover:text-primary hover:bg-primary/10 rounded-lg transition-colors"
                          aria-label="Редагувати"
                        >
                          <Edit2 size={16} />
                        </button>
                        <button 
                          onClick={() => deletePhantom(p.phantomId)}
                          className="p-2 text-[#7a7a7a] hover:text-red-500 hover:bg-red-50 rounded-lg transition-colors"
                          aria-label="Видалити"
                        >
                          <Trash2 size={16} />
                        </button>
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      )}
    </Card>
  );
};
