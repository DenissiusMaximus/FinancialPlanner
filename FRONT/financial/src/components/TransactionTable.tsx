import React from 'react';
import {
  getCurrencyDisplay,
  getTransactionTypeLabel,
  getTransactionTypeColor,
  isExpenseType,
} from '../utils/display-helpers';

interface Transaction {
  id: number;
  amount: number;
  date: string;
  comment?: string;
  category: {
    id: number;
    name: string;
  } | null;
  source: {
    id: number;
    name: string;
  };
  transactionType: {
    id: number;
    name: string;
  };
  currency: {
    id: number;
    name: string;
  };
}

interface TransactionTableProps {
  transactions: Transaction[];
  onEdit?: (transaction: Transaction) => void;
  onDelete?: (id: number) => void;
}

const IconEdit = () => (
  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
    <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
  </svg>
);

const IconTrash = () => (
  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <polyline points="3 6 5 6 21 6" /><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6" />
    <path d="M10 11v6" /><path d="M14 11v6" /><path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2" />
  </svg>
);

export const TransactionTable: React.FC<TransactionTableProps> = ({
  transactions,
  onEdit,
  onDelete,
}) => {

  return (
    <div className="w-full overflow-x-auto">
      <table className="w-full border-collapse text-sm">
        <thead>
          <tr className="bg-[#f5f5f7] border-b border-hairline">
            <th className="px-4 py-3 text-left font-semibold text-ink text-xs uppercase tracking-wider">Дата</th>
            <th className="px-4 py-3 text-left font-semibold text-ink text-xs uppercase tracking-wider">Категорія</th>
            <th className="px-4 py-3 text-left font-semibold text-ink text-xs uppercase tracking-wider">Тип</th>
            <th className="px-4 py-3 text-left font-semibold text-ink text-xs uppercase tracking-wider">Сума</th>
            <th className="px-4 py-3 text-left font-semibold text-ink text-xs uppercase tracking-wider">Джерело</th>
            {(onEdit || onDelete) && (
              <th className="px-4 py-3 text-left font-semibold text-ink text-xs uppercase tracking-wider">Дії</th>
            )}
          </tr>
        </thead>
        <tbody>
          {transactions.map((transaction) => (
            <tr key={transaction.id} className="border-b border-[#f0f0f0] hover:bg-[#fafafc] transition-colors">
              <td className="px-4 py-3 text-[#7a7a7a] font-mono text-xs">
                {new Date(transaction.date).toLocaleDateString('uk-UA')}
              </td>
              <td className="px-4 py-3 text-ink">{transaction.category?.name || '—'}</td>
              <td className="px-4 py-3">
                <span
                  className="font-medium text-xs px-2 py-0.5 rounded-full inline-flex items-center"
                  style={{
                    color: getTransactionTypeColor(transaction.transactionType.name),
                    backgroundColor: getTransactionTypeColor(transaction.transactionType.name) + '18',
                  }}
                >
                  {getTransactionTypeLabel(transaction.transactionType.name).icon}
                  {getTransactionTypeLabel(transaction.transactionType.name).label}
                </span>
              </td>
              <td
                className="px-4 py-3 font-mono font-semibold text-sm"
                style={{ color: getTransactionTypeColor(transaction.transactionType.name) }}
              >
                {isExpenseType(transaction.transactionType.name) ? '-' : '+'}
                {transaction.amount.toFixed(2)} {getCurrencyDisplay(transaction.currency?.name)}
              </td>
              <td className="px-4 py-3 text-ink">{transaction.source?.name ?? '—'}</td>
              {(onEdit || onDelete) && (
                <td className="px-4 py-3">
                  <div className="flex gap-1">
                    {onEdit && (
                      <button
                        className="text-[#7a7a7a] hover:text-primary transition-colors p-1.5 rounded-lg hover:bg-primary/5"
                        onClick={() => onEdit(transaction)}
                        title="Редагувати"
                      >
                        <IconEdit />
                      </button>
                    )}
                    {onDelete && (
                      <button
                        className="text-[#7a7a7a] hover:text-red-500 transition-colors p-1.5 rounded-lg hover:bg-red-50"
                        onClick={() => onDelete(transaction.id)}
                        title="Видалити"
                      >
                        <IconTrash />
                      </button>
                    )}
                  </div>
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>

      {transactions.length === 0 && (
        <div className="py-8 text-center text-[#7a7a7a]">
          <p>Транзакцій не знайдено</p>
        </div>
      )}
    </div>
  );
};
