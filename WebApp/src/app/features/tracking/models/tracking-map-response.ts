export interface TrackingMapResponse {
  readonly orderId: string;
  readonly restaurantLatitude: number;
  readonly restaurantLongitude: number;
  readonly destinationLatitude: number | null;
  readonly destinationLongitude: number | null;
  readonly destinationAddress: string | null;
  readonly courierLatitude: number | null;
  readonly courierLongitude: number | null;
  readonly progress: number;
  readonly trackingStatus: string;
  readonly estimatedSecondsRemaining: number | null;
  readonly updatedAt: string | null;
  readonly message: string | null;
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null;
}

function isDateOrNull(value: unknown): value is string | null {
  return value === null
    || (typeof value === 'string' && !Number.isNaN(Date.parse(value)));
}

function isNullableCoordinate(
  value: unknown,
  minimum: number,
  maximum: number
): value is number | null {
  return value === null
    || (typeof value === 'number'
      && Number.isFinite(value)
      && value >= minimum
      && value <= maximum);
}

export function parseTrackingMapResponse(value: unknown): TrackingMapResponse {
  if (!isRecord(value)) {
    throw new Error('Malformed tracking-map response.');
  }

  const orderId = value['orderId'];
  const restaurantLatitude = value['restaurantLatitude'];
  const restaurantLongitude = value['restaurantLongitude'];
  const destinationLatitude = value['destinationLatitude'];
  const destinationLongitude = value['destinationLongitude'];
  const destinationAddress = value['destinationAddress'];
  const courierLatitude = value['courierLatitude'];
  const courierLongitude = value['courierLongitude'];
  const progress = value['progress'];
  const trackingStatus = value['trackingStatus'];
  const estimatedSecondsRemaining = value['estimatedSecondsRemaining'];
  const updatedAt = value['updatedAt'];
  const message = value['message'];

  if (
    typeof orderId !== 'string'
    || !/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(orderId)
    || typeof restaurantLatitude !== 'number'
    || !isNullableCoordinate(restaurantLatitude, -90, 90)
    || typeof restaurantLongitude !== 'number'
    || !isNullableCoordinate(restaurantLongitude, -180, 180)
    || !isNullableCoordinate(destinationLatitude, -90, 90)
    || !isNullableCoordinate(destinationLongitude, -180, 180)
    || !(destinationAddress === null || typeof destinationAddress === 'string')
    || !isNullableCoordinate(courierLatitude, -90, 90)
    || !isNullableCoordinate(courierLongitude, -180, 180)
    || typeof progress !== 'number'
    || !Number.isFinite(progress)
    || progress < 0
    || progress > 1
    || typeof trackingStatus !== 'string'
    || trackingStatus.trim().length === 0
    || !(estimatedSecondsRemaining === null
      || (typeof estimatedSecondsRemaining === 'number'
        && Number.isInteger(estimatedSecondsRemaining)
        && estimatedSecondsRemaining >= 0))
    || !isDateOrNull(updatedAt)
    || !(message === null || typeof message === 'string')
  ) {
    throw new Error('Malformed tracking-map response.');
  }

  return {
    orderId,
    restaurantLatitude,
    restaurantLongitude,
    destinationLatitude,
    destinationLongitude,
    destinationAddress,
    courierLatitude,
    courierLongitude,
    progress,
    trackingStatus,
    estimatedSecondsRemaining,
    updatedAt,
    message
  };
}
