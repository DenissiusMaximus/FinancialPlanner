import React from 'react';
import { Modal } from './Modal';
import { Button } from './Button';

interface ConfirmModalProps {
  isOpen: boolean;
  title: string;
  message: string;
  onConfirm: () => void;
  onCancel: () => void;
  confirmText?: string;
  cancelText?: string;
}

export const ConfirmModal: React.FC<ConfirmModalProps> = ({
  isOpen,
  title,
  message,
  onConfirm,
  onCancel,
  confirmText = 'Видалити',
  cancelText = 'Скасувати'
}) => {
  return (
    <Modal isOpen={isOpen} title={title} onClose={onCancel} size="sm">
      <p className="text-[#333333] mb-6 text-sm">{message}</p>
      <div className="flex justify-end gap-3">
        <Button variant="tertiary" onClick={onCancel}>
          {cancelText}
        </Button>
        <Button variant="danger" onClick={onConfirm}>
          {confirmText}
        </Button>
      </div>
    </Modal>
  );
};
