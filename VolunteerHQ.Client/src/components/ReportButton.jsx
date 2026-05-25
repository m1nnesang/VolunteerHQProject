import { useState } from 'react'
import { Link } from 'react-router-dom'
import toast from 'react-hot-toast'
import { createReport } from '../api/reports'
import { useAuth } from '../context/AuthContext'

const categories = [
  { value: 'Spam', label: 'Спам' },
  { value: 'Abuse', label: 'Образа / неприпустимий контент' },
  { value: 'Fraud', label: 'Шахрайство' },
  { value: 'Other', label: 'Інше' },
]

export default function ReportButton({ reportedId, label = 'Поскаржитись' }) {
  const { user, isUser } = useAuth()
  const [open, setOpen] = useState(false)
  const [form, setForm] = useState({ category: 'Spam', reason: '' })
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')

  const handleSubmit = async (e) => {
    e.preventDefault()
    if (form.reason.trim().length < 10) {
      setError('Опишіть причину (мін. 10 символів)')
      return
    }
    setError('')
    setSubmitting(true)
    try {
      await createReport({
        reportedId,
        category: form.category,
        reason: form.reason.trim(),
      })
      setOpen(false)
      setForm({ category: 'Spam', reason: '' })
      toast.success('Скаргу відправлено адміністрації')
    } catch (err) {
      toast.error(err.response?.data?.error || 'Помилка при відправці')
    } finally {
      setSubmitting(false)
    }
  }

  if (!user) {
    return (
      <Link to="/login" className="text-gray-600 hover:text-red-400 text-xs transition-colors">
        🚩 {label}
      </Link>
    )
  }

  if (!isUser) return null

  return (
    <>
      <button
        onClick={() => setOpen(true)}
        className="text-gray-600 hover:text-red-400 text-xs transition-colors"
      >
        🚩 {label}
      </button>

      {open && (
        <div className="fixed inset-0 bg-black/70 flex items-center justify-center px-4 z-50" onClick={() => setOpen(false)}>
          <div
            className="bg-[#1c1d24] border border-[#2e303a] rounded-2xl p-6 w-full max-w-md"
            onClick={e => e.stopPropagation()}
          >
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-white font-semibold">Поскаржитись</h3>
              <button onClick={() => setOpen(false)} className="text-gray-500 hover:text-white">✕</button>
            </div>

            <form onSubmit={handleSubmit} className="space-y-4">
              {error && (
                <div className="bg-red-500/10 border border-red-500/20 text-red-400 text-sm rounded-lg px-4 py-2">
                  {error}
                </div>
              )}

                <div>
                  <label className="block text-gray-400 text-sm mb-2">Категорія</label>
                  <select
                    value={form.category}
                    onChange={e => setForm(prev => ({ ...prev, category: e.target.value }))}
                    className="w-full bg-[#111218] border border-[#2e303a] rounded-lg px-4 py-2.5 text-white text-sm focus:outline-none focus:border-yellow-400"
                  >
                    {categories.map(c => (
                      <option key={c.value} value={c.value} className="bg-[#1c1d24]">
                        {c.label}
                      </option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-gray-400 text-sm mb-2">Опис проблеми</label>
                  <textarea
                    value={form.reason}
                    onChange={e => setForm(prev => ({ ...prev, reason: e.target.value }))}
                    placeholder="Опишіть детально що сталось..."
                    rows={4}
                    maxLength={500}
                    className="w-full bg-[#111218] border border-[#2e303a] rounded-lg px-4 py-3 text-white text-sm placeholder-gray-600 focus:outline-none focus:border-yellow-400 resize-none"
                  />
                  <div className="text-right text-xs text-gray-600 mt-1">{form.reason.length}/500</div>
                </div>

                <div className="flex gap-2 justify-end pt-2">
                  <button
                    type="button"
                    onClick={() => setOpen(false)}
                    className="text-gray-400 text-sm px-4 py-2 hover:text-white transition-colors"
                  >
                    Скасувати
                  </button>
                  <button
                    type="submit"
                    disabled={submitting}
                    className="bg-red-400/10 border border-red-400/30 text-red-400 text-sm font-semibold px-5 py-2 rounded-lg hover:bg-red-400/20 transition-colors disabled:opacity-50"
                  >
                    {submitting ? 'Відправляємо...' : 'Відправити скаргу'}
                  </button>
                </div>
              </form>
          </div>
        </div>
      )}
    </>
  )
}
