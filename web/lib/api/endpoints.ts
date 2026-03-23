export const apiEndpoints = {
  login: "auth/login",
  refresh: "auth/refresh",
  logout: "auth/logout",
  createShipment: "shipments",
  shipmentStatusTransitions: (shipmentId: string) => `shipments/${shipmentId}/status-transitions`,
  searchShipments: "search/shipments",
  trackingSummary: (trackingCode: string) => `tracking/${trackingCode}`,
  trackingTimeline: (trackingCode: string) => `tracking/${trackingCode}/timeline`,
};