export interface CourierLocation {
  readonly orderId: string;
  readonly trackingSessionId: string;
  readonly latitude: number;
  readonly longitude: number;
  readonly progress: number;
  readonly trackingStatus: string;
  readonly estimatedSecondsRemaining: number;
  readonly updatedAt: string;
}

const GUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null;
}

function isCoordinate(latitude: number, longitude: number): boolean {
  return Number.isFinite(latitude)
    && latitude >= -90
    && latitude <= 90
    && Number.isFinite(longitude)
    && longitude >= -180
    && longitude <= 180;
}

export function parseCourierLocation(value: unknown): CourierLocation | null {
  if (!isRecord(value)) {
    return null;
  }

  const orderId = value['orderId'];
  const trackingSessionId = value['trackingSessionId'];
  const latitude = value['latitude'];
  const longitude = value['longitude'];
  const progress = value['progress'];
  const trackingStatus = value['trackingStatus'];
  const estimatedSecondsRemaining = value['estimatedSecondsRemaining'];
  const updatedAt = value['updatedAt'];

  if (
    typeof orderId !== 'string'
    || !GUID_PATTERN.test(orderId)
    || typeof trackingSessionId !== 'string'
    || !GUID_PATTERN.test(trackingSessionId)
    || typeof latitude !== 'number'
    || typeof longitude !== 'number'
    || !isCoordinate(latitude, longitude)
    || typeof progress !== 'number'
    || !Number.isFinite(progress)
    || progress < 0
    || progress > 1
    || typeof trackingStatus !== 'string'
    || trackingStatus.trim().length === 0
    || typeof estimatedSecondsRemaining !== 'number'
    || !Number.isInteger(estimatedSecondsRemaining)
    || estimatedSecondsRemaining < 0
    || typeof updatedAt !== 'string'
    || Number.isNaN(Date.parse(updatedAt))
  ) {
    return null;
  }

  return {
    orderId,
    trackingSessionId,
    latitude,
    longitude,
    progress,
    trackingStatus,
    estimatedSecondsRemaining,
    updatedAt
  };
}
