import { useEffect, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import { getOrganization, createJoinRequest } from '../api/organizations'
import { useAuth } from '../context/AuthContext'
import SubscribeButton from '../components/SubscribeButton'
import ReportButton from '../components/ReportButton'

export default function OrganizationDetailPage() {
  const { id } = useParams()
  const { user } = useAuth()
  const [org, setOrg] = useState(null)
  const [loading, setLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [form, setForm] = useState({ bio: '', skills: '', experience: '', cvFilePath: '' })
  const [submitting, setSubmitting] = useState(false)
  const [success, setSuccess] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    getOrganization(id)
      .then(res => setOrg(res.data))
      .catch(console.error)
      .finally(() => setLoading(false))
  }, [id])

  const handleChange = (e) => {
    setForm(prev => ({ ...prev, [e.target.name]: e.target.value }))
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    setSubmitting(true)
    try {
      const res = await createJoinRequest(id, {
        bio: form.bio,
        skills: form.skills,
        experience: form.experience,
        cvFilePath: form.cvFilePath || '',
      })
      // Зберігаємо id заявки + orgId для перегляду статусу в профілі
      localStorage.setItem('myJoinRequest', JSON.stringify({
        requestId: res.data.id,
        orgId: parseInt(id),
        orgName: org?.name,
      }))
      setSuccess(true)
      setShowForm(false)
    } catch (err) {
      setError(err.response?.data?.error || 'Помилка при подачі заявки')
    } finally {
      setSubmitting(false)
    }
  }

  const myMembership = org?.members?.find(m => m.userId === user?.userId)
  const isMember = !!myMembership
  const canManage = ['Leader', 'Deputy'].includes(myMembership?.role)

  if (loading) {
    return (
      <div className="min-h-screen bg-[#111218] flex items-center justify-center">
        <div className="text-gray-500">Завантаження...</div>
      </div>
    )
  }

  if (!org) {
    return (
      <div className="min-h-screen bg-[#111218] flex items-center justify-center">
        <div className="text-gray-500">Організацію не знайдено</div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-[#111218]">
      <div className="max-w-3xl mx-auto px-4 py-10">

        {/* Header */}
        <div className="bg-[#1c1d24] border border-[#2e303a] rounded-2xl p-8 mb-6">
          <div className="flex items-start justify-between gap-4 mb-6">
            <div className="flex items-center gap-5 min-w-0">
              <div className="w-16 h-16 bg-yellow-400/20 rounded-2xl flex items-center justify-center text-yellow-400 text-2xl font-bold shrink-0">
                {org.name?.charAt(0).toUpperCase()}
              </div>
              <div className="min-w-0">
                <h1 className="text-white text-2xl font-bold">{org.name}</h1>
                <p className="text-gray-500 text-sm mt-1">📍 {org.city}</p>
              </div>
            </div>
            <SubscribeButton target="Organization" targetId={parseInt(id)} />
          </div>
          <p className="text-gray-400 leading-relaxed">{org.description}</p>
          <div className="flex items-center justify-between mt-4">
            <div className="flex items-center gap-3">
              <span className="text-xs text-gray-600">
                Створено {new Date(org.createdAt).toLocaleDateString('uk-UA', { day: 'numeric', month: 'long', year: 'numeric' })}
              </span>
              <span className="text-gray-700">·</span>
              <ReportButton reportedId={parseInt(id)} label="Поскаржитись" />
            </div>
            {canManage && (
              <Link
                to={`/organizations/${id}/manage`}
                className="text-xs text-gray-500 hover:text-yellow-400 transition-colors"
              >
                Керування →
              </Link>
            )}
          </div>
        </div>

        {/* Join block */}
        {success ? (
          <div className="bg-green-500/10 border border-green-500/20 text-green-400 rounded-xl px-6 py-4 text-sm">
            ✅ Заявку подано! Очікуйте на розгляд від керівництва організації.
          </div>
        ) : isMember ? (
          <div className="bg-[#1c1d24] border border-[#2e303a] rounded-xl px-6 py-4 flex items-center justify-between">
            <div>
              <div className="text-white text-sm font-medium">Ви учасник цієї організації</div>
              <div className="text-gray-500 text-xs mt-1">
                Роль: {myMembership?.role === 'Leader' ? 'Лідер' : myMembership?.role === 'Deputy' ? 'Заступник' : 'Учасник'}
              </div>
            </div>
            {canManage && (
              <Link
                to={`/organizations/${id}/manage`}
                className="bg-yellow-400 text-black text-sm font-semibold px-5 py-2 rounded-lg hover:bg-yellow-300 transition-colors"
              >
                Керування
              </Link>
            )}
          </div>
        ) : user ? (
          <div className="bg-[#1c1d24] border border-[#2e303a] rounded-2xl p-6">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-white font-semibold">Вступити до організації</h2>
              {!showForm && (
                <button
                  onClick={() => setShowForm(true)}
                  className="bg-yellow-400 text-black text-sm font-semibold px-5 py-2 rounded-lg hover:bg-yellow-300 transition-colors"
                >
                  Подати заявку
                </button>
              )}
            </div>

            {showForm && (
              <form onSubmit={handleSubmit} className="space-y-4">
                {error && (
                  <div className="bg-red-500/10 border border-red-500/20 text-red-400 text-sm rounded-lg px-4 py-3">
                    {error}
                  </div>
                )}
                <div>
                  <label className="block text-gray-400 text-sm mb-2">Про себе</label>
                  <textarea
                    name="bio"
                    value={form.bio}
                    onChange={handleChange}
                    placeholder="Розкажіть про себе..."
                    rows={3}
                    required
                    className="w-full bg-[#111218] border border-[#2e303a] rounded-lg px-4 py-3 text-white text-sm placeholder-gray-600 focus:outline-none focus:border-yellow-400 transition-colors resize-none"
                  />
                </div>
                <div>
                  <label className="block text-gray-400 text-sm mb-2">Навички</label>
                  <input
                    type="text"
                    name="skills"
                    value={form.skills}
                    onChange={handleChange}
                    placeholder="Медицина, логістика, IT..."
                    required
                    className="w-full bg-[#111218] border border-[#2e303a] rounded-lg px-4 py-3 text-white text-sm placeholder-gray-600 focus:outline-none focus:border-yellow-400 transition-colors"
                  />
                </div>
                <div>
                  <label className="block text-gray-400 text-sm mb-2">Досвід волонтерства</label>
                  <textarea
                    name="experience"
                    value={form.experience}
                    onChange={handleChange}
                    placeholder="Опишіть ваш попередній досвід..."
                    rows={3}
                    required
                    className="w-full bg-[#111218] border border-[#2e303a] rounded-lg px-4 py-3 text-white text-sm placeholder-gray-600 focus:outline-none focus:border-yellow-400 transition-colors resize-none"
                  />
                </div>
                <div className="flex gap-3 pt-2">
                  <button
                    type="submit"
                    disabled={submitting}
                    className="bg-yellow-400 text-black font-semibold px-6 py-2.5 rounded-lg hover:bg-yellow-300 transition-colors disabled:opacity-50 text-sm"
                  >
                    {submitting ? 'Відправляємо...' : 'Відправити заявку'}
                  </button>
                  <button
                    type="button"
                    onClick={() => setShowForm(false)}
                    className="border border-[#2e303a] text-gray-400 px-6 py-2.5 rounded-lg hover:border-gray-500 transition-colors text-sm"
                  >
                    Скасувати
                  </button>
                </div>
              </form>
            )}
          </div>
        ) : (
          <div className="bg-[#1c1d24] border border-[#2e303a] rounded-xl px-6 py-5 flex items-center justify-between">
            <p className="text-gray-400 text-sm">Увійдіть щоб подати заявку до організації</p>
            <Link
              to="/login"
              className="bg-yellow-400 text-black text-sm font-semibold px-5 py-2 rounded-lg hover:bg-yellow-300 transition-colors shrink-0"
            >
              Увійти
            </Link>
          </div>
        )}

        {isMember && org.members?.length > 0 && (
          <div className="bg-[#1c1d24] border border-[#2e303a] rounded-2xl p-6 mt-6">
            <h2 className="text-white font-semibold mb-4">
              Учасники <span className="text-gray-600 font-normal text-sm">({org.members.length})</span>
            </h2>
            <div className="space-y-2">
              {org.members.map(m => {
                const roleLabel = m.role === 'Leader' ? 'Лідер'
                  : m.role === 'Deputy' ? 'Заступник'
                  : 'Учасник'
                const isMe = m.userId === user?.userId
                const fullName = m.firstName && m.secondName
                  ? `${m.firstName} ${m.secondName}`
                  : `Користувач #${m.userId}`
                const initials = m.firstName && m.secondName
                  ? `${m.firstName.charAt(0)}${m.secondName.charAt(0)}`.toUpperCase()
                  : `#${m.userId}`
                return (
                  <div key={m.id} className="flex items-center justify-between py-2 border-b border-[#2e303a] last:border-0">
                    <div className="flex items-center gap-3">
                      <div className="w-8 h-8 bg-yellow-400/20 rounded-full flex items-center justify-center text-yellow-400 text-xs font-bold shrink-0">
                        {initials}
                      </div>
                      <div>
                        <span className="text-white text-sm">
                          {fullName}
                          {isMe && <span className="text-gray-600 text-xs ml-1">(ви)</span>}
                        </span>
                        <div className="text-gray-600 text-xs">{roleLabel}</div>
                      </div>
                    </div>
                    {!isMe && (
                      <Link
                        to={`/messages/${m.userId}`}
                        className="text-gray-600 hover:text-yellow-400 transition-colors text-sm"
                        title="Написати"
                      >
                        ✉
                      </Link>
                    )}
                  </div>
                )
              })}
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
