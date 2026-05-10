import React, { ReactNode } from 'react';
import { ArrowDownToLine, ArrowUpFromLine, RefreshCw, Pencil, HelpCircle } from 'lucide-react';
import { TransactionTypeEnum } from '../types/enums';

// Currency icons mapping
const currencyIcons: Record<string, string> = {
  // Major currencies
  USD: '$',
  EUR: '€',
  GBP: '£',
  JPY: '¥',
  
  // Ukrainian
  UAH: '₴',
  
  // Other popular
  CNY: '¥',
  RUB: '₽',
  CAD: '$',
  AUD: '$',
  CHF: '₣',
  SEK: 'kr',
  NOK: 'kr',
  DKK: 'kr',
  PLN: 'zł',
  CZK: 'Kč',
  HUF: 'Ft',
  RON: 'lei',
  BGN: 'лв',
  HRK: 'kn',
  RSD: 'дин',
  INR: '₹',
  BRL: 'R$',
  MXN: '$',
  AZN: '₼',
  KZK: '₸',
};

// Transaction type translations and icons
const transactionTypes: Record<string, { label: string; icon: ReactNode }> = {
  [TransactionTypeEnum.INCOME.toLowerCase()]: { label: 'Надходження', icon: <ArrowDownToLine size={14} className="mr-1" /> },
  [TransactionTypeEnum.EXPENSE.toLowerCase()]: { label: 'Витрата', icon: <ArrowUpFromLine size={14} className="mr-1" /> },
  [TransactionTypeEnum.TRANSFER.toLowerCase()]: { label: 'Переказ', icon: <RefreshCw size={14} className="mr-1" /> },
  [TransactionTypeEnum.ADJUSTMENT.toLowerCase()]: { label: 'Уточнення', icon: <Pencil size={14} className="mr-1" /> },
  // Fallbacks for Ukrainian returned from API just in case
  'надходження': { label: 'Надходження', icon: <ArrowDownToLine size={14} className="mr-1" /> },
  'витрата': { label: 'Витрата', icon: <ArrowUpFromLine size={14} className="mr-1" /> },
  'переказ': { label: 'Переказ', icon: <RefreshCw size={14} className="mr-1" /> },
  'уточнення': { label: 'Уточнення', icon: <Pencil size={14} className="mr-1" /> },
};

/**
 * Get currency display (icon only)
 */
export const getCurrencyDisplay = (currencyName?: string): string => {
  if (!currencyName) return '💱';
  const code = currencyName.toUpperCase();
  return currencyIcons[code] ?? `${code}`;
};

export const getCurrencyCode = (currencyName?: string): string => {
  if (!currencyName) return '---';
  return currencyName.toUpperCase();
};

/**
 * Get transaction type label with icon
 */
export const getTransactionTypeLabel = (typeName?: string): { label: string; icon: ReactNode } => {
  if (!typeName) return { label: 'Невідомо', icon: <HelpCircle size={14} className="mr-1" /> };
  const key = typeName.toLowerCase();
  return transactionTypes[key] ?? { label: typeName, icon: <span className="mr-1">•</span> };
};

/**
 * Get color for transaction type
 */
export const getTransactionTypeColor = (typeName?: string): string => {
  if (!typeName) return '#7a7a7a';
  const key = typeName.toLowerCase();
  
  if (key === TransactionTypeEnum.INCOME.toLowerCase() || key === 'надходження') return '#34c759';
  if (key === TransactionTypeEnum.EXPENSE.toLowerCase() || key === 'витрата') return '#ff3b30';
  if (key === TransactionTypeEnum.TRANSFER.toLowerCase() || key === 'переказ') return '#0066cc';
  if (key === TransactionTypeEnum.ADJUSTMENT.toLowerCase() || key === 'уточнення') return '#ff9500';
  
  return '#7a7a7a';
};

/**
 * Check if transaction is an expense (for sign in amount)
 */
export const isExpenseType = (typeName?: string): boolean => {
  if (!typeName) return false;
  const key = typeName.toLowerCase();
  return key === TransactionTypeEnum.EXPENSE.toLowerCase() || key === 'витрата';
};

/**
 * Check if transaction is an income
 */
export const isIncomeType = (typeName?: string): boolean => {
  if (!typeName) return false;
  const key = typeName.toLowerCase();
  return key === TransactionTypeEnum.INCOME.toLowerCase() || key === 'надходження';
};
