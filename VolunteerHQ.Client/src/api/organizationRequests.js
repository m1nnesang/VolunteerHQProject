import api from './client'

export const createOrganizationRequest = (dto) =>
  api.post('/organizationrequest', dto)

export const getMyOrganizationRequests = (page = 1, pageSize = 20) =>
  api.get('/organizationrequest/all', { params: { page, pageSize } })
