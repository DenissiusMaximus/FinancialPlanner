import type { SourceDtoDetailed, SourceDtoLookup } from '../types/generated';

export const calculateTotalAmount = (sources: (SourceDtoDetailed | SourceDtoLookup)[] | any): number => {
  if (!Array.isArray(sources)) return 0;
  return sources.reduce((sum: number, source: any) => sum + (source?.amount || 0), 0);
};

export const calculateAimProgress = (
  collectedAmount: number,
  targetAmount: number
): number => {
  if (targetAmount === 0) return 0;
  return (collectedAmount / targetAmount) * 100;
};

export const calculateTotalAimsProgress = (
  aimsData: Array<{
    progress: {
      collectedAmount: number;
      targetAmount: number;
    };
  }>
): number => {
  if (aimsData.length === 0) return 0;

  const totalCollected = aimsData.reduce((sum, aim) => sum + aim.progress.collectedAmount, 0);
  const totalTarget = aimsData.reduce((sum, aim) => sum + aim.progress.targetAmount, 0);

  if (totalTarget === 0) return 0;
  return (totalCollected / totalTarget) * 100;
};

export const groupTransactionsByType = (
  transactions: Array<{ transactionType: { name: string }; amount: number }>
) => {
  return transactions.reduce(
    (acc, transaction) => {
      const type = transaction.transactionType.name.toLowerCase();
      if (type === 'income') {
        acc.income += transaction.amount;
      } else if (type === 'expense') {
        acc.expense += transaction.amount;
      }
      return acc;
    },
    { income: 0, expense: 0 }
  );
};
