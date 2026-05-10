export const TransactionTypeEnum = {
  INCOME: 'Income',
  EXPENSE: 'Expense',
  TRANSFER: 'Transfer',
  ADJUSTMENT: 'Adjustment',
} as const;

export type TransactionTypeEnum = typeof TransactionTypeEnum[keyof typeof TransactionTypeEnum];
