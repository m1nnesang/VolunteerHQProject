import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import toast from 'react-hot-toast'
import { useAuth } from '../context/AuthContext'
import { formatUah } from '../utils/money'
import { getStats } from '../api/stats'
import {
  getAllReports,
  reviewReport,
  getAuditLogs,
  getAllOrganizationRequests,
  reviewOrganizationRequest,
  createMilitaryUnit,
} from '../api/admin'

const reportStatusLabels = {
  Pending: { label: 'На розгляді', color: 'bg-yellow-400/10 text-yellow-400 border-yellow-400/20' },
  Reviewed: { label: 'Розглянуто', color: 'bg-green-400/10 text-green-400 border-green-400/20' },
  Dismissed: { label: 'Відхилено', color: 'bg-gray-400/10 text-gray-400 border-gray-400/20' },
}

const categoryLabels = {
  Spam: 'Спам',
  Abuse: 'Образа',
  Fraud: 'Шахрайство',
  Other: 'Інше',
}

const requestStatusLabels = {
  Pending: { label: 'На розгляді', color: 'bg-yellow-400/10 text-yellow-400 border-yellow-400/20' },
  Approved: { label: 'Схвалено', color: 'bg-green-400/10 text-green-400 border-green-400/20' },
  Rejected: { label: 'Відхилено', color: 'bg-red-400/10 text-red-400 border-red-400/20' },
}

export default function AdminPage() {
  const { user, isAdmin } = useAuth()
  const navigate = useNavigate()
  const [tab, setTab] = useState('overview')
  const [stats, setStats] = useState(null)
  const [reports, setReports] = useState([])
  const [logs, setLogs] = useState([])
  const [orgRequests, setOrgRequests] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!user) {
      navigate('/login')
      return
    }
    if (!isAdmin) return
    loadAll()
  }, [user])

  const loadAll = async () => {
    setLoading(true)
    setError('')
    try {
      const [statsRes, reportsRes, logsRes, orgReqRes] = await Promise.all([
        getStats(),
        getAllReports(1, 100).catch(() => ({ data: { items: [] } })),
        getAuditLogs(1, 50).catch(() => ({ data: { items: [] } })),
        getAllOrganizationRequests(1, 100).catch(() => ({ data: { items: [] } })),
      ])
      setStats(statsRes.data)
      setReports(reportsRes.data?.items || [])
      setLogs(logsRes.data?.items || [])
      setOrgRequests(orgReqRes.data?.items || [])
    } catch (err) {
      setError(err.response?.data?.error || 'Помилка завантаження')
    } finally {
      setLoading(false)
    }
  }

  const handleReviewReport = async (id, status) => {
    try {
      await reviewReport(id, { status, adminComment: null })
      setReports(prev => prev.map(r => r.id === id ? { ...r, status } : r))
      toast.success('Скаргу оброблено')
    } catch (err) {
      toast.error(err.response?.data?.error || 'Помилка')
    }
  }

  const handleReviewOrgRequest = async (id, status) => {
    try {
      await reviewOrganizationRequest(id, { status, adminComment: null })
      setOrgRequests(prev => prev.map(r => r.id === id ? { ...r, status } : r))
      toast.success(status === 'Approved' ? 'Заявку схвалено' : 'Заявку відхилено')
    } catch (err) {
      toast.error(err.response?.data?.error || 'Помилка')
    }
  }

  if (!isAdmin) {
    return (
      <div className="min-h-screen bg-[#111218] flex items-center justify-center px-4">
        <div className="text-center">
          <p className="text-gray-400 mb-2">Доступ заборонено</p>
          <p className="text-gray-600 text-sm">Тільки для адміністраторів</p>
        </div>
      </div>
    )
  }

  const pendingReports = reports.filter(r => r.status === 'Pending').length
  const pendingOrgRequests = orgRequests.filter(r => r.status === 'Pending').length

  return (
    <div className="min-h-screen bg-[#111218]">
      <div className="max-w-6xl mx-auto px-4 py-10">
        <div className="mb-8">
          <h1 className="text-white text-3xl font-bold mb-2">Адмін-панель</h1>
          <p className="text-gray-500 text-sm">Керування платформою</p>
        </div>

        {error && (
          <div className="bg-red-500/10 border border-red-500/20 text-red-400 text-sm rounded-lg px-4 py-3 mb-6">
            {error}
          </div>
        )}

        {/* Tabs */}
        <div className="flex gap-1 mb-6 border-b border-[#2e303a]">
          {[
            { key: 'overview', label: 'Огляд' },
            { key: 'orgRequests', label: 'Заявки на організації', badge: pendingOrgRequests },
            { key: 'reports', label: 'Скарги', badge: pendingReports },
            { key: 'units', label: 'Підрозділи' },
            { key: 'logs', label: 'AuditLog' },
          ].map(({ key, label, badge }) => (
            <button
              key={key}
              onClick={() => setTab(key)}
              className={`px-4 py-3 text-sm font-medium transition-colors border-b-2 -mb-px whitespace-nowrap ${
                tab === key
                  ? 'text-white border-yellow-400'
                  : 'text-gray-500 border-transparent hover:text-gray-300'
              }`}
            >
              {label}
              {badge > 0 && (
                <span className="ml-2 bg-yellow-400 text-black text-xs font-bold px-1.5 rounded-full">
                  {badge}
                </span>
              )}
            </button>
          ))}
        </div>

        {loading ? (
          <div className="text-gray-500">Завантаження...</div>
        ) : tab === 'overview' ? (
          <OverviewTab stats={stats} pendingReports={pendingReports} pendingOrgRequests={pendingOrgRequests} />
        ) : tab === 'reports' ? (
          <ReportsTab reports={reports} onReview={handleReviewReport} />
        ) : tab === 'orgRequests' ? (
          <OrgRequestsTab requests={orgRequests} onReview={handleReviewOrgRequest} />
        ) : tab === 'units' ? (
          <CreateUnitTab />
        ) : (
          <LogsTab logs={logs} />
        )}
      </div>
    </div>
  )
}

function OverviewTab({ stats, pendingReports, pendingOrgRequests }) {
  return (
    <div className="space-y-6">
      <div className="grid md:grid-cols-4 gap-4">
        {[
          { label: 'Зібрано всього', value: stats ? formatUah(stats.totalDonated) : '—' },
          { label: 'Активних зборів', value: stats?.activeFundraisers ?? '—' },
          { label: 'Завершених', value: stats?.completedFundraisers ?? '—' },
          { label: 'Унікальних донорів', value: stats?.uniqueDonors ?? '—' },
        ].map(({ label, value }) => (
          <div key={label} className="bg-[#1c1d24] border border-[#2e303a] rounded-xl p-5">
            <div className="text-gray-500 text-xs mb-2">{label}</div>
            <div className="text-2xl font-bold text-white">{value}</div>
          </div>
        ))}
      </div>

      {(pendingReports > 0 || pendingOrgRequests > 0) && (
        <div className="bg-yellow-400/5 border border-yellow-400/20 rounded-xl p-5">
          <h3 className="text-yellow-400 font-semibold mb-2">⚠ Потребують уваги</h3>
          <ul className="text-sm text-gray-300 space-y-1">
            {pendingOrgRequests > 0 && <li>• {pendingOrgRequests} заявка(и) на створення організації</li>}
            {pendingReports > 0 && <li>• {pendingReports} нових скарг</li>}
          </ul>
        </div>
      )}
    </div>
  )
}

function ReportsTab({ reports, onReview }) {
  if (reports.length === 0) {
    return <div className="bg-[#1c1d24] border border-[#2e303a] rounded-xl p-12 text-center text-gray-500 text-sm">Скарг немає</div>
  }
  return (
    <div className="space-y-3">
      {reports.map(r => {
        const status = reportStatusLabels[r.status] || { label: r.status, color: '' }
        return (
          <div key={r.id} className="bg-[#1c1d24] border border-[#2e303a] rounded-xl p-5">
            <div className="flex items-center justify-between mb-3">
              <div className="flex items-center gap-3">
                <span className="text-xs px-2.5 py-1 rounded-full bg-red-400/10 text-red-400 border border-red-400/20 font-medium">
                  {categoryLabels[r.category] || r.category}
                </span>
                <span className="text-gray-500 text-xs">
                  Скарга #{r.id} · від #{r.reporterId ?? '?'} на #{r.reportedId ?? '?'}
                </span>
              </div>
              <span className={`text-xs px-3 py-1 rounded-full font-medium border ${status.color}`}>
                {status.label}
              </span>
            </div>
            <p className="text-gray-300 text-sm leading-relaxed mb-3">{r.reason}</p>
            <div className="flex items-center justify-between">
              <span className="text-gray-600 text-xs">
                {new Date(r.createdAt).toLocaleString('uk-UA')}
              </span>
              {r.status === 'Pending' && (
                <div className="flex gap-2">
                  <button
                    onClick={() => onReview(r.id, 'Reviewed')}
                    className="bg-green-400/10 border border-green-400/30 text-green-400 text-sm px-4 py-1.5 rounded-lg hover:bg-green-400/20 transition-colors"
                  >
                    ✓ Розглянуто
                  </button>
                  <button
                    onClick={() => onReview(r.id, 'Dismissed')}
                    className="bg-gray-400/10 border border-gray-400/30 text-gray-400 text-sm px-4 py-1.5 rounded-lg hover:bg-gray-400/20 transition-colors"
                  >
                    Відхилити
                  </button>
                </div>
              )}
            </div>
          </div>
        )
      })}
    </div>
  )
}

function OrgRequestsTab({ requests, onReview }) {
  if (requests.length === 0) {
    return <div className="bg-[#1c1d24] border border-[#2e303a] rounded-xl p-12 text-center text-gray-500 text-sm">Заявок немає</div>
  }
  return (
    <div className="space-y-3">
      {requests.map(r => {
        const status = requestStatusLabels[r.status] || { label: r.status, color: '' }
        return (
          <div key={r.id} className="bg-[#1c1d24] border border-[#2e303a] rounded-xl p-5">
            <div className="flex items-start justify-between mb-3">
              <div className="min-w-0">
                <h3 className="text-white font-semibold">{r.proposedName}</h3>
                <p className="text-gray-500 text-sm mt-0.5">📍 {r.city} · від #{r.userId}</p>
              </div>
              <span className={`text-xs px-3 py-1 rounded-full font-medium border ${status.color} shrink-0`}>
                {status.label}
              </span>
            </div>

            <p className="text-gray-400 text-sm mb-3 leading-relaxed">{r.description}</p>

            <div className="bg-[#111218] border border-[#2e303a] rounded-lg p-3 mb-3 space-y-1">
              <div className="text-xs"><span className="text-gray-600">Про себе:</span> <span className="text-gray-400">{r.bio}</span></div>
              <div className="text-xs"><span className="text-gray-600">Навички:</span> <span className="text-gray-400">{r.skills}</span></div>
              <div className="text-xs"><span className="text-gray-600">Досвід:</span> <span className="text-gray-400">{r.experience}</span></div>
            </div>

            {r.status === 'Pending' && (
              <div className="flex gap-2">
                <button
                  onClick={() => onReview(r.id, 'Approved')}
                  className="bg-green-400/10 border border-green-400/30 text-green-400 text-sm px-4 py-1.5 rounded-lg hover:bg-green-400/20 transition-colors"
                >
                  ✓ Схвалити
                </button>
                <button
                  onClick={() => onReview(r.id, 'Rejected')}
                  className="bg-red-400/10 border border-red-400/30 text-red-400 text-sm px-4 py-1.5 rounded-lg hover:bg-red-400/20 transition-colors"
                >
                  ✕ Відхилити
                </button>
              </div>
            )}
          </div>
        )
      })}
    </div>
  )
}

function CreateUnitTab() {
  const [form, setForm] = useState({ login: '', password: '', unitName: '', contactPerson: '', isNameHidden: false })
  const [submitting, setSubmitting] = useState(false)
  const [success, setSuccess] = useState(null)
  const [error, setError] = useState('')

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target
    setForm(prev => ({ ...prev, [name]: type === 'checkbox' ? checked : value }))
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    setSuccess(null)
    setSubmitting(true)
    const loginUsed = form.login
    try {
      const res = await createMilitaryUnit({
        login: form.login,
        password: form.password,
        unitName: form.unitName,
        contactPerson: form.contactPerson,
        isNameHidden: form.isNameHidden,
      })
      setSuccess({ ...res.data, login: loginUsed })
      setForm({ login: '', password: '', unitName: '', contactPerson: '', isNameHidden: false })
    } catch (err) {
      setError(err.response?.data?.error || 'Помилка при створенні')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="max-w-lg">
      <h2 className="text-white font-semibold text-lg mb-5">Створити військовий підрозділ</h2>

      {success && (
        <div className="bg-green-500/10 border border-green-500/20 text-green-400 rounded-xl px-5 py-4 text-sm mb-5">
          ✅ Підрозділ <span className="font-semibold">{success.unitName}</span> створено. Логін: <span className="font-mono">{success.login}</span>
        </div>
      )}

      {error && (
        <div className="bg-red-500/10 border border-red-500/20 text-red-400 rounded-xl px-5 py-4 text-sm mb-5">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit} className="bg-[#1c1d24] border border-[#2e303a] rounded-2xl p-6 space-y-4">
        <div>
          <label className="block text-gray-400 text-sm mb-2">Логін</label>
          <input
            type="text"
            name="login"
            value={form.login}
            onChange={handleChange}
            required
            className="w-full bg-[#111218] border border-[#2e303a] rounded-lg px-4 py-3 text-white text-sm placeholder-gray-600 focus:outline-none focus:border-yellow-400 transition-colors"
          />
        </div>
        <div>
          <label className="block text-gray-400 text-sm mb-2">Пароль</label>
          <input
            type="password"
            name="password"
            value={form.password}
            onChange={handleChange}
            required
            className="w-full bg-[#111218] border border-[#2e303a] rounded-lg px-4 py-3 text-white text-sm placeholder-gray-600 focus:outline-none focus:border-yellow-400 transition-colors"
          />
        </div>
        <div>
          <label className="block text-gray-400 text-sm mb-2">Назва підрозділу</label>
          <input
            type="text"
            name="unitName"
            value={form.unitName}
            onChange={handleChange}
            required
            className="w-full bg-[#111218] border border-[#2e303a] rounded-lg px-4 py-3 text-white text-sm placeholder-gray-600 focus:outline-none focus:border-yellow-400 transition-colors"
          />
        </div>
        <div>
          <label className="block text-gray-400 text-sm mb-2">Контактна особа</label>
          <input
            type="text"
            name="contactPerson"
            value={form.contactPerson}
            onChange={handleChange}
            required
            className="w-full bg-[#111218] border border-[#2e303a] rounded-lg px-4 py-3 text-white text-sm placeholder-gray-600 focus:outline-none focus:border-yellow-400 transition-colors"
          />
        </div>
        <div className="flex items-center gap-3">
          <input
            type="checkbox"
            name="isNameHidden"
            id="isNameHidden"
            checked={form.isNameHidden}
            onChange={handleChange}
            className="w-4 h-4 accent-yellow-400"
          />
          <label htmlFor="isNameHidden" className="text-gray-400 text-sm">Приховати назву підрозділу</label>
        </div>
        <button
          type="submit"
          disabled={submitting}
          className="w-full bg-yellow-400 text-black font-semibold py-3 rounded-lg hover:bg-yellow-300 transition-colors disabled:opacity-50 text-sm"
        >
          {submitting ? 'Створення...' : 'Створити підрозділ'}
        </button>
      </form>
    </div>
  )
}

function LogsTab({ logs }) {
  if (logs.length === 0) {
    return <div className="bg-[#1c1d24] border border-[#2e303a] rounded-xl p-12 text-center text-gray-500 text-sm">Логів немає</div>
  }
  return (
    <div className="bg-[#1c1d24] border border-[#2e303a] rounded-xl overflow-hidden">
      <table className="w-full text-sm">
        <thead className="border-b border-[#2e303a]">
          <tr className="text-left text-gray-500 text-xs uppercase">
            <th className="px-4 py-3 font-medium">Час</th>
            <th className="px-4 py-3 font-medium">Користувач</th>
            <th className="px-4 py-3 font-medium">Дія</th>
            <th className="px-4 py-3 font-medium">Сутність</th>
            <th className="px-4 py-3 font-medium">Деталі</th>
          </tr>
        </thead>
        <tbody>
          {logs.map(log => (
            <tr key={log.id} className="border-b border-[#2e303a] hover:bg-[#111218] transition-colors">
              <td className="px-4 py-3 text-gray-400 text-xs whitespace-nowrap">
                {new Date(log.createdAt).toLocaleString('uk-UA')}
              </td>
              <td className="px-4 py-3 text-gray-400">#{log.userId}</td>
              <td className="px-4 py-3 text-yellow-400 text-xs font-medium">{log.action}</td>
              <td className="px-4 py-3 text-gray-400 text-xs">{log.entityType} #{log.entityId}</td>
              <td className="px-4 py-3 text-gray-500 text-xs">{log.details}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
