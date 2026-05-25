import api from './client'

// Reports
export const getAllReports = (page = 1, pageSize = 50) =>
  api.get('/report/all', { params: { page, pageSize } })

export const reviewReport = (reportId, dto) =>
  api.put(`/report/${reportId}/review`, dto)

// AuditLog
export const getAuditLogs = (page = 1, pageSize = 50) =>
  api.get('/auditlog', { params: { page, pageSize } })

// OrganizationRequests (admin)
export const getAllOrganizationRequests = (page = 1, pageSize = 50) =>
  api.get('/organizationrequest/all', { params: { page, pageSize } })

export const reviewOrganizationRequest = (requestId, dto) =>
  api.put(`/organizationrequest/${requestId}/review`, dto)

// MilitaryUnit
export const createMilitaryUnit = (dto) =>
  api.post('/militaryunit/create', dto)
