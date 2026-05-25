import api from './client'

export const getFundraisers = (page = 1, pageSize = 20) =>
  api.get('/fundraiser', { params: { page, pageSize } })

export const getFundraiser = (id) => api.get(`/fundraiser/${id}`)

export const directDonate = (fundraiserId, dto) =>
  api.post(`/fundraiser/${fundraiserId}/donate`, dto)

export const createFundraiser = (dto) => api.post('/fundraiser', dto)

export const assignOrganization = (fundraiserId, orgId) =>
  api.post(`/fundraiser/${fundraiserId}/assign`, null, { params: { orgId } })

export const donateViaCode = (fundraiserId, uniqueCode, dto) =>
  api.post(`/fundraiser/donate/${uniqueCode}`, dto, { params: { fundraiserId } })
