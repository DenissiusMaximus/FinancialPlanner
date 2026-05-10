// Дефолтні значення коефіцієнтів обрізування/лімітів
export const DEFAULT_PAGINATION_LIMIT = 10;
export const AIMS_PREVIEW_COUNT = 5;
export const TRANSACTIONS_PREVIEW_COUNT = 5;

// Дефолтна валюта
export const DEFAULT_CURRENCY = 'UAH';

// Константи для модалів
export const MODAL_SIZES = {
  small: 'sm',
  medium: 'md',
  large: 'lg',
} as const;

// Типи транзакцій
export const TRANSACTION_TYPES = {
  INCOME: 'Income',
  EXPENSE: 'Expense',
  TRANSFER: 'Transfer',
} as const;

// Сортування
export const SORT_OPTIONS = {
  DATE_DESC: { by: 'Date', descending: true },
  DATE_ASC: { by: 'Date', descending: false },
  AMOUNT_DESC: { by: 'Amount', descending: true },
  AMOUNT_ASC: { by: 'Amount', descending: false },
} as const;
