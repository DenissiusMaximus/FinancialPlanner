import React, { useState, useEffect } from 'react';
import { X, Calendar as CalendarIcon } from 'lucide-react';
import type { PhantomTransaction } from '../hooks/usePhantomTransactions';
import { useCurrencyConvert } from '../hooks/useCurrencyConvert';

interface PhantomTransactionModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSave: (transaction: Omit<PhantomTransaction, 'phantomId' | 'isEnabled'>) => void;
  editingTransaction?: PhantomTransaction | null;
}

export const PhantomTransactionModal: React.FC<PhantomTransactionModalProps> = ({
  isOpen, onClose, onSave, editingTransaction
}) => {
  const { selectedCurrencyName } = useCurrencyConvert();
  
  const [name, setName] = useState('');
  const [amount, setAmount] = useState<string>('');
  const [type, setType] = useState<'Дохід' | 'Витрата'>('Витрата');
  const [frequency, setFrequency] = useState('Одноразово');
  const [customDays, setCustomDays] = useState<string>('14');
  const [startDate, setStartDate] = useState(new Date().toISOString().split('T')[0]);

  useEffect(() => {
    if (isOpen && editingTransaction) {
      setName(editingTransaction.name || '');
      setAmount(editingTransaction.amount?.toString() || '');
      setType(editingTransaction.transactionType?.name?.includes('Дохід') ? 'Дохід' : 'Витрата');
      
      const freqName = editingTransaction.frequency?.name || 'Одноразово';
      if (['Одноразово', 'Щодня', 'Щотижня', 'Щомісяця', 'Щороку'].includes(freqName)) {
        setFrequency(freqName);
      } else if (freqName === 'Довільні дні') {
        setFrequency('Довільні дні');
        setCustomDays(editingTransaction.frequency?.intervalValue?.toString() || '14');
      } else {
        setFrequency('Одноразово');
      }

      if (editingTransaction.startDate) {
        setStartDate(new Date(editingTransaction.startDate).toISOString().split('T')[0]);
      }
    } else if (isOpen) {
      // Reset form
      setName('');
      setAmount('');
      setType('Витрата');
      setFrequency('Одноразово');
      setCustomDays('14');
      setStartDate(new Date().toISOString().split('T')[0]);
    }
  }, [isOpen, editingTransaction]);

  if (!isOpen) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!name || !amount) return;

    let intervalValue = 1;
    let intervalUnitName = 'Day';

    if (frequency === 'Одноразово') { intervalValue = 9999; }
    else if (frequency === 'Щодня') { intervalValue = 1; intervalUnitName = 'Day'; }
    else if (frequency === 'Щотижня') { intervalValue = 1; intervalUnitName = 'Week'; }
    else if (frequency === 'Щомісяця') { intervalValue = 1; intervalUnitName = 'Month'; }
    else if (frequency === 'Щороку') { intervalValue = 1; intervalUnitName = 'Year'; }
    else if (frequency === 'Довільні дні') { intervalValue = parseInt(customDays) || 1; intervalUnitName = 'Day'; }

    const newTrans: Omit<PhantomTransaction, 'phantomId' | 'isEnabled'> = {
      name,
      amount: parseFloat(amount),
      transactionType: { id: type === 'Дохід' ? 1 : 2, name: type },
      frequency: { 
        name: frequency, 
        intervalValue, 
        intervalUnit: { id: 1, name: intervalUnitName } 
      },
      startDate: new Date(startDate).toISOString(),
      currency: { id: 1, name: selectedCurrencyName } as any, // Mock currency, converter will use selectedCurrencyName later if needed
    };

    onSave(newTrans);
    onClose();
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-ink/40 backdrop-blur-sm animate-in fade-in duration-200">
      <div className="bg-white rounded-3xl w-full max-w-md shadow-2xl overflow-hidden animate-in zoom-in-95 duration-200">
        <div className="flex justify-between items-center p-5 border-b border-hairline">
          <h2 className="text-xl font-semibold text-ink">
            {editingTransaction ? 'Редагувати фантом' : 'Нова фантомна транзакція'}
          </h2>
          <button 
            onClick={onClose}
            className="p-2 text-[#7a7a7a] hover:bg-[#f5f5f7] rounded-full transition-colors"
          >
            <X size={20} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-5 space-y-5">
          {/* Type Selector (Segmented Control) */}
          <div className="flex p-1 bg-[#f5f5f7] rounded-xl">
            {(['Витрата', 'Дохід'] as const).map((t) => (
              <button
                key={t}
                type="button"
                onClick={() => setType(t)}
                className={`flex-1 py-2 text-sm font-semibold rounded-lg transition-all ${
                  type === t 
                    ? 'bg-white text-ink shadow-sm' 
                    : 'text-[#7a7a7a] hover:text-ink'
                }`}
              >
                {t}
              </button>
            ))}
          </div>

          {/* Name & Amount */}
          <div className="space-y-4">
            <div>
              <label className="block text-xs font-semibold text-[#7a7a7a] uppercase mb-1.5 ml-1">Назва</label>
              <input
                required
                type="text"
                placeholder="Наприклад: Нові джинси"
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="w-full px-4 py-3 rounded-xl border border-[#e5e5ea] bg-white focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary transition-all text-ink"
              />
            </div>
            
            <div>
              <label className="block text-xs font-semibold text-[#7a7a7a] uppercase mb-1.5 ml-1">Сума</label>
              <div className="relative">
                <input
                  required
                  type="number"
                  step="0.01"
                  min="0"
                  placeholder="0.00"
                  value={amount}
                  onChange={(e) => setAmount(e.target.value)}
                  className="w-full pl-4 pr-16 py-3 rounded-xl border border-[#e5e5ea] bg-white focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary transition-all text-ink font-mono"
                />
                <span className="absolute right-4 top-1/2 -translate-y-1/2 text-[#7a7a7a] font-semibold">
                  {selectedCurrencyName}
                </span>
              </div>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            {/* Frequency */}
            <div className="col-span-2 sm:col-span-1">
              <label className="block text-xs font-semibold text-[#7a7a7a] uppercase mb-1.5 ml-1">Повторення</label>
              <select
                value={frequency}
                onChange={(e) => setFrequency(e.target.value)}
                className="w-full px-4 py-3 rounded-xl border border-[#e5e5ea] bg-white focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary transition-all text-ink appearance-none"
                style={{ backgroundImage: `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 24 24' stroke='%237a7a7a'%3E%3Cpath stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M19 9l-7 7-7-7'%3E%3C/path সীম>%3C/svg%3E")`, backgroundPosition: 'right 12px center', backgroundRepeat: 'no-repeat', backgroundSize: '16px' }}
              >
                <option value="Одноразово">Одноразово</option>
                <option value="Щодня">Щодня</option>
                <option value="Щотижня">Щотижня</option>
                <option value="Щомісяця">Щомісяця</option>
                <option value="Щороку">Щороку</option>
                <option value="Довільні дні">Довільні дні</option>
              </select>
            </div>

            {/* Start Date */}
            <div className="col-span-2 sm:col-span-1">
              <label className="block text-xs font-semibold text-[#7a7a7a] uppercase mb-1.5 ml-1">Дата початку</label>
              <div className="relative">
                <input
                  required
                  type="date"
                  value={startDate}
                  onChange={(e) => setStartDate(e.target.value)}
                  className="w-full px-4 py-3 rounded-xl border border-[#e5e5ea] bg-white focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary transition-all text-ink appearance-none"
                />
              </div>
            </div>
          </div>

          {frequency === 'Довільні дні' && (
            <div className="animate-in fade-in slide-in-from-top-2">
              <label className="block text-xs font-semibold text-[#7a7a7a] uppercase mb-1.5 ml-1">Кожні (днів)</label>
              <input
                required
                type="number"
                min="1"
                value={customDays}
                onChange={(e) => setCustomDays(e.target.value)}
                className="w-full px-4 py-3 rounded-xl border border-[#e5e5ea] bg-white focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary transition-all text-ink font-mono"
              />
            </div>
          )}

          <div className="pt-4">
            <button
              type="submit"
              className="w-full bg-primary text-white font-semibold py-3.5 rounded-xl hover:bg-primary/90 active:scale-[0.98] transition-all shadow-sm shadow-primary/20"
            >
              {editingTransaction ? 'Зберегти зміни' : 'Створити фантом'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
