export const apiEndpoints = {
  login: "auth/login",
  searchShipments: "search/shipments",
  trackingSummary: (trackingCode: string) => `tracking/${trackingCode}`,
  trackingTimeline: (trackingCode: string) => `tracking/${trackingCode}/timeline`,
};