import { useState, useEffect } from 'react';
import type { PlannedTransactionDto } from '../types/generated';

export interface PhantomTransaction extends PlannedTransactionDto {
  phantomId: string;
  isEnabled: boolean;
}

const STORAGE_KEY = 'financial_planner_phantom_transactions';
const STORAGE_TIMESTAMP_KEY = 'financial_planner_phantom_timestamp';
const TTL_HOURS = 24;

export const usePhantomTransactions = () => {
  const [phantoms, setPhantoms] = useState<PhantomTransaction[]>([]);

  useEffect(() => {
    try {
      const storedTimestampStr = localStorage.getItem(STORAGE_TIMESTAMP_KEY);
      const now = Date.now();

      if (storedTimestampStr) {
        const storedTimestamp = parseInt(storedTimestampStr, 10);
        const hoursPassed = (now - storedTimestamp) / (1000 * 60 * 60);

        // Clear if it's older than 24 hours
        if (hoursPassed >= TTL_HOURS) {
          localStorage.removeItem(STORAGE_KEY);
          localStorage.removeItem(STORAGE_TIMESTAMP_KEY);
          setPhantoms([]);
          return;
        }
      }

      const storedStr = localStorage.getItem(STORAGE_KEY);
      if (storedStr) {
        setPhantoms(JSON.parse(storedStr));
      }
    } catch (e) {
      console.error('Failed to load phantom transactions', e);
    }
  }, []);

  const saveToStorage = (data: PhantomTransaction[]) => {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(data));
      // Update timestamp on every save so the session extends 24 hours from last activity
      localStorage.setItem(STORAGE_TIMESTAMP_KEY, Date.now().toString());
    } catch (e) {
      console.error('Failed to save phantom transactions', e);
    }
  };

  const addPhantom = (transaction: Omit<PhantomTransaction, 'phantomId' | 'isEnabled'>) => {
    const newPhantom: PhantomTransaction = {
      ...transaction,
      phantomId: crypto.randomUUID(),
      isEnabled: true,
      // Default to "Phantom" category if none provided so it shows clearly
      category: transaction.category || { id: -1, name: 'Фантомна категорія' },
    };
    const updated = [...phantoms, newPhantom];
    setPhantoms(updated);
    saveToStorage(updated);
  };

  const editPhantom = (phantomId: string, updates: Partial<PhantomTransaction>) => {
    const updated = phantoms.map((p) => 
      p.phantomId === phantomId ? { ...p, ...updates } : p
    );
    setPhantoms(updated);
    saveToStorage(updated);
  };

  const deletePhantom = (phantomId: string) => {
    const updated = phantoms.filter((p) => p.phantomId !== phantomId);
    setPhantoms(updated);
    saveToStorage(updated);
  };

  const togglePhantom = (phantomId: string) => {
    const updated = phantoms.map((p) => 
      p.phantomId === phantomId ? { ...p, isEnabled: !p.isEnabled } : p
    );
    setPhantoms(updated);
    saveToStorage(updated);
  };

  const clearAllPhantoms = () => {
    setPhantoms([]);
    localStorage.removeItem(STORAGE_KEY);
    localStorage.removeItem(STORAGE_TIMESTAMP_KEY);
  };

  return {
    phantoms,
    addPhantom,
    editPhantom,
    deletePhantom,
    togglePhantom,
    clearAllPhantoms,
  };
};
