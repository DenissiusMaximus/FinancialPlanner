// Utility функції для роботи з валютами
export const convertCurrency = (amount: number, fromRate: number, toRate: number): number => {
  if (fromRate === 0 || toRate === 0) return amount;
  return (amount / fromRate) * toRate;
};

export const formatCurrency = (amount: number, decimals = 2): string => {
  return amount.toFixed(decimals);
};

export const formatDate = (date: string | Date, locale = 'uk-UA'): string => {
  return new Date(date).toLocaleDateString(locale);
};

export const getLocalDatetime = (dateStr?: string | Date): string => {
  const d = dateStr ? new Date(dateStr) : new Date();
  if (isNaN(d.getTime())) return '';
  const pad = (n: number) => n.toString().padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
};

export const getTransactionTypeColor = (type: string): string => {
  const colorMap: Record<string, string> = {
    income: '#34c759',
    expense: '#ff3b30',
    transfer: '#0066cc',
  };
  return colorMap[type.toLowerCase()] || '#7a7a7a';
};

export const getTransactionTypeSign = (type: string): string => {
  const typeMap: Record<string, string> = {
    income: '+',
    expense: '-',
    transfer: '→',
  };
  return typeMap[type.toLowerCase()] || '';
};
