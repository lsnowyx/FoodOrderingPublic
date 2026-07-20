export type TrackingConnectionState =
  | 'disconnected'
  | 'connecting'
  | 'connected'
  | 'reconnecting'
  | 'error';

export type TimelineStepState = 'completed' | 'current' | 'upcoming' | 'cancelled';

export interface TimelineStep {
  readonly key: string;
  readonly label: string;
  readonly state: TimelineStepState;
}

const ORDER_STATUS_SEQUENCE = [
  { key: 'Pending', label: 'Order received' },
  { key: 'Paid', label: 'Payment confirmed' },
  { key: 'Preparing', label: 'Preparing your order' },
  { key: 'OutForDelivery', label: 'Out for delivery' },
  { key: 'Delivered', label: 'Delivered' }
] as const;

const ORDER_STATUS_LABELS: Readonly<Record<string, string>> = {
  Pending: 'Order received',
  Paid: 'Payment confirmed',
  Preparing: 'Preparing your order',
  OutForDelivery: 'Out for delivery',
  Delivered: 'Delivered',
  Cancelled: 'Cancelled'
};

const PAYMENT_METHOD_LABELS: Readonly<Record<string, string>> = {
  CashOnDelivery: 'Cash on delivery',
  OnlineCard: 'Online card'
};

const PAYMENT_STATUS_LABELS: Readonly<Record<string, string>> = {
  Unpaid: 'Unpaid',
  PendingOnlinePayment: 'Online payment pending',
  Paid: 'Paid',
  Failed: 'Payment failed',
  Expired: 'Payment link expired'
};

const TRACKING_STATUS_LABELS: Readonly<Record<string, string>> = {
  NotStarted: 'Delivery not started',
  InProgress: 'Courier en route',
  Arrived: 'Courier arrived',
  Cancelled: 'Delivery cancelled'
};

function humanize(value: string): string {
  const normalized = value.trim();
  if (normalized.length === 0) {
    return 'Unknown';
  }

  return normalized
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .replace(/[_-]+/g, ' ')
    .replace(/^./, character => character.toUpperCase());
}

export function getOrderStatusLabel(status: string): string {
  return ORDER_STATUS_LABELS[status] ?? humanize(status);
}

export function getPaymentMethodLabel(method: string): string {
  return PAYMENT_METHOD_LABELS[method] ?? humanize(method);
}

export function getPaymentStatusLabel(status: string): string {
  return PAYMENT_STATUS_LABELS[status] ?? humanize(status);
}

export function getTrackingStatusLabel(status: string): string {
  return TRACKING_STATUS_LABELS[status] ?? humanize(status);
}

export function isTerminalOrderStatus(status: string): boolean {
  return status === 'Delivered' || status === 'Cancelled';
}

export function createOrderTimeline(status: string): readonly TimelineStep[] {
  if (status === 'Cancelled') {
    return [
      {
        key: 'Pending',
        label: 'Order received',
        state: 'completed'
      },
      {
        key: 'Cancelled',
        label: 'Order cancelled',
        state: 'cancelled'
      }
    ];
  }

  const currentIndex = ORDER_STATUS_SEQUENCE.findIndex(step => step.key === status);
  if (currentIndex === -1) {
    return [
      ...ORDER_STATUS_SEQUENCE.map(step => ({
        ...step,
        state: 'upcoming' as const
      })),
      {
        key: status,
        label: getOrderStatusLabel(status),
        state: 'current' as const
      }
    ];
  }

  return ORDER_STATUS_SEQUENCE.map((step, index) => ({
    ...step,
    state: status === 'Delivered' || index < currentIndex
      ? 'completed'
      : index === currentIndex
        ? 'current'
        : 'upcoming'
  }));
}
