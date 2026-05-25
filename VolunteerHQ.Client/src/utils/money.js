const NBSP = ' '

export function formatMoney(value) {
  if (value === null || value === undefined || value === '') return ''
  const digits = String(value).replace(/\D/g, '')
  if (!digits) return ''
  return digits.replace(/\B(?=(\d{3})+(?!\d))/g, NBSP)
}

export function unformatMoney(value) {
  return String(value ?? '').replace(/\D/g, '')
}

export function formatUah(value) {
  if (value === null || value === undefined) return `0${NBSP}₴`
  const num = Number(value)
  if (!Number.isFinite(num)) return `0${NBSP}₴`
  const formatted = Math.round(num).toLocaleString('uk-UA').replace(/\s/g, NBSP)
  return `${formatted}${NBSP}₴`
}
