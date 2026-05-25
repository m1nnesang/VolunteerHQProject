import { useEffect, useState } from 'react'
import { useParams, Link } from 'react-router-dom'
import {
  getOrganization,
  getOrganizationMembers,
  removeMember,
  updateMemberRole,
  getJoinRequestsForOrg,
  reviewJoinRequest,
} from '../api/organizations'
import { useAuth } from '../context/AuthContext'

const roleLabels = {
  Leader: { label: 'Лідер', color: 'bg-yellow-400/10 text-yellow-400 border-yellow-400/20' },
  Deputy: { label: 'Заступник', color: 'bg-blue-400/10 text-blue-400 border-blue-400/20' },
  Member: { label: 'Учасник', color: 'bg-gray-400/10 text-gray-400 border-gray-400/20' },
}

const statusLabels = {
  Pending: { label: 'Очікує', color: 'bg-yellow-400/10 text-yellow-400 border-yellow-400/20' },
  Approved: { label: 'Схвалено', color: 'bg-green-400/10 text-green-400 border-green-400/20' },
  Rejected: { label: 'Відхилено', color: 'bg-red-400/10 text-red-400 border-red-400/20' },
}

export default function OrganizationManagePage() {
  const { id } = useParams()
  const orgId = parseInt(id)
  const { user } = useAuth()
  const [tab, setTab] = useState('requests')
  const [org, setOrg] = useState(null)
  const [members, setMembers] = useState([])
  const [requests, setRequests] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const load = async () => {
    setLoading(true)
    setError('')
    try {
      const [orgRes, membersRes, requestsRes] = await Promise.all([
        getOrganization(orgId),
        getOrganizationMembers(orgId, 1, 100),
        getJoinRequestsForOrg(orgId, 1, 100).catch(() => ({ data: { items: [] } })),
      ])
      setOrg(orgRes.data)
      setMembers(membersRes.data?.items || [])
      setRequests(requestsRes.data?.items || [])
    } catch (err) {
      setError(err.response?.data?.error || 'Помилка завантаження')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [orgId])

  const handleReview = async (requestId, status) => {
    try {
      await reviewJoinRequest(requestId, orgId, { status, adminComment: null })
      load()
    } catch (err) {
      alert(err.response?.data?.error || 'Помилка')
    }
  }

  const handleRemove = async (userId) => {
    if (!confirm('Видалити цього учасника?')) return
    try {
      await removeMember(orgId, userId)
      setMembers(prev => prev.filter(m => m.userId !== userId))
    } catch (err) {
      alert(err.response?.data?.error || 'Помилка')
    }
  }

  const handleRoleChange = async (userId, newRole) => {
    try {
      await updateMemberRole(orgId, userId, newRole)
      setMembers(prev => prev.map(m => m.userId === userId ? { ...m, role: newRole } : m))
    } catch (err) {
      alert(err.response?.data?.error || 'Помилка')
    }
  }

  if (!user) {
    return (
      <div className="min-h-screen bg-[#111218] flex items-center justify-center px-4">
        <div className="text-center">
          <p className="text-gray-400 mb-4">Увійдіть для керування організацією</p>
          <Link to="/login" className="text-yellow-400 hover:text-yellow-300">Увійти</Link>
        </div>
      </div>
    )
  }

  const pendingRequests = requests.filter(r => r.status === 'Pending')

  return (
    <div className="min-h-screen bg-[#111218]">
      <div className="max-w-4xl mx-auto px-4 py-10">
        {/* Header */}
        <div className="mb-6">
          <Link to={`/organizations/${orgId}`} className="text-gray-500 text-sm hover:text-gray-300">
            ← До організації
          </Link>
          <h1 className="text-white text-3xl font-bold mt-2">Керування</h1>
          {org && <p className="text-gray-500 text-sm mt-1">{org.name}</p>}
        </div>

        {error && (
          <div className="bg-red-500/10 border border-red-500/20 text-red-400 text-sm rounded-lg px-4 py-3 mb-6">
            {error}
          </div>
        )}

        {/* Tabs */}
        <div className="flex gap-2 mb-6 border-b border-[#2e303a]">
          <button
            onClick={() => setTab('requests')}
            className={`px-4 py-3 text-sm font-medium transition-colors border-b-2 -mb-px ${
              tab === 'requests'
                ? 'text-white border-yellow-400'
                : 'text-gray-500 border-transparent hover:text-gray-300'
            }`}
          >
            Заявки {pendingRequests.length > 0 && (
              <span className="ml-1 bg-yellow-400 text-black text-xs font-bold px-1.5 rounded-full">
                {pendingRequests.length}
              </span>
            )}
          </button>
          <button
            onClick={() => setTab('members')}
            className={`px-4 py-3 text-sm font-medium transition-colors border-b-2 -mb-px ${
              tab === 'members'
                ? 'text-white border-yellow-400'
                : 'text-gray-500 border-transparent hover:text-gray-300'
            }`}
          >
            Учасники <span className="text-gray-600">({members.length})</span>
          </button>
        </div>

        {/* Content */}
        {loading ? (
          <div className="space-y-3">
            {[1, 2, 3].map(i => <div key={i} className="bg-[#1c1d24] rounded-xl h-20 animate-pulse" />)}
          </div>
        ) : tab === 'requests' ? (
          <div className="space-y-3">
            {requests.length === 0 ? (
              <div className="bg-[#1c1d24] border border-[#2e303a] rounded-xl p-12 text-center text-gray-500 text-sm">
                Заявок поки немає
              </div>
            ) : (
              requests.map(req => {
                const status = statusLabels[req.status] || { label: req.status, color: '' }
                const fullName = req.firstName && req.secondName
                  ? `${req.firstName} ${req.secondName}`
                  : `Користувач #${req.userId}`
                const initials = req.firstName && req.secondName
                  ? `${req.firstName.charAt(0)}${req.secondName.charAt(0)}`.toUpperCase()
                  : `#${req.userId}`
                return (
                  <div key={req.id} className="bg-[#1c1d24] border border-[#2e303a] rounded-xl p-5">
                    <div className="flex items-center justify-between mb-3">
                      <div className="flex items-center gap-3">
                        <div className="w-10 h-10 bg-yellow-400/20 rounded-full flex items-center justify-center text-yellow-400 text-xs font-bold">
                          {initials}
                        </div>
                        <div>
                          <div className="text-white font-medium text-sm">{fullName}</div>
                          <div className="text-gray-600 text-xs">
                            {new Date(req.createdAt).toLocaleDateString('uk-UA', {
                              day: 'numeric', month: 'short', year: 'numeric'
                            })}
                          </div>
                        </div>
                      </div>
                      <span className={`text-xs px-3 py-1 rounded-full font-medium border ${status.color}`}>
                        {status.label}
                      </span>
                    </div>

                    {(req.bio || req.skills || req.experience) && (
                      <div className="bg-[#111218] border border-[#2e303a] rounded-lg p-3 mb-3 space-y-1.5">
                        {req.bio && (
                          <div className="text-xs"><span className="text-gray-600">Про себе:</span> <span className="text-gray-400">{req.bio}</span></div>
                        )}
                        {req.skills && (
                          <div className="text-xs"><span className="text-gray-600">Навички:</span> <span className="text-gray-400">{req.skills}</span></div>
                        )}
                        {req.experience && (
                          <div className="text-xs"><span className="text-gray-600">Досвід:</span> <span className="text-gray-400">{req.experience}</span></div>
                        )}
                      </div>
                    )}

                    {req.status === 'Pending' && (
                      <div className="flex gap-2">
                        <button
                          onClick={() => handleReview(req.id, 'Approved')}
                          className="bg-green-400/10 border border-green-400/30 text-green-400 text-sm px-4 py-2 rounded-lg hover:bg-green-400/20 transition-colors"
                        >
                          ✓ Схвалити
                        </button>
                        <button
                          onClick={() => handleReview(req.id, 'Rejected')}
                          className="bg-red-400/10 border border-red-400/30 text-red-400 text-sm px-4 py-2 rounded-lg hover:bg-red-400/20 transition-colors"
                        >
                          ✕ Відхилити
                        </button>
                      </div>
                    )}
                  </div>
                )
              })
            )}
          </div>
        ) : (
          <div className="space-y-3">
            {members.length === 0 ? (
              <div className="bg-[#1c1d24] border border-[#2e303a] rounded-xl p-12 text-center text-gray-500 text-sm">
                Поки немає учасників
              </div>
            ) : (
              members.map(member => {
                const role = roleLabels[member.role] || { label: member.role, color: '' }
                const isMe = member.userId === user.userId
                const fullName = member.firstName && member.secondName
                  ? `${member.firstName} ${member.secondName}`
                  : `Користувач #${member.userId}`
                const initials = member.firstName && member.secondName
                  ? `${member.firstName.charAt(0)}${member.secondName.charAt(0)}`.toUpperCase()
                  : `#${member.userId}`
                return (
                  <div key={member.id} className="bg-[#1c1d24] border border-[#2e303a] rounded-xl p-4 flex items-center gap-4">
                    <div className="w-10 h-10 bg-yellow-400/20 rounded-full flex items-center justify-center text-yellow-400 text-xs font-bold shrink-0">
                      {initials}
                    </div>
                    <div className="flex-1 min-w-0">
                      <div className="text-white font-medium text-sm flex items-center gap-2">
                        {fullName}
                        {isMe && <span className="text-xs text-gray-600">(ви)</span>}
                      </div>
                      <div className="text-gray-600 text-xs">
                        З {new Date(member.joinedAt).toLocaleDateString('uk-UA', {
                          month: 'long', year: 'numeric'
                        })}
                      </div>
                    </div>

                    {member.role === 'Leader' ? (
                      <span className={`text-xs px-3 py-1.5 rounded-lg border ${role.color}`}>
                        {roleLabels.Leader.label}
                      </span>
                    ) : (
                      <select
                        value={member.role}
                        onChange={(e) => handleRoleChange(member.userId, e.target.value)}
                        disabled={isMe}
                        className={`bg-[#111218] border text-xs px-2 py-1.5 rounded-lg focus:outline-none focus:border-yellow-400 transition-colors disabled:opacity-50 ${role.color.replace('bg-', 'border-').split(' ')[2] || 'border-[#2e303a]'}`}
                      >
                        {Object.keys(roleLabels)
                          .filter(r => r !== 'Leader')
                          .map(r => (
                            <option key={r} value={r} className="bg-[#1c1d24] text-white">
                              {roleLabels[r].label}
                            </option>
                          ))}
                      </select>
                    )}

                    {!isMe && (
                      <Link
                        to={`/messages/${member.userId}`}
                        className="text-gray-600 hover:text-yellow-400 transition-colors text-sm"
                        title="Написати"
                      >
                        ✉
                      </Link>
                    )}

                    {!isMe && (
                      <button
                        onClick={() => handleRemove(member.userId)}
                        className="text-gray-600 hover:text-red-400 transition-colors text-sm"
                        title="Видалити"
                      >
                        ✕
                      </button>
                    )}
                  </div>
                )
              })
            )}
          </div>
        )}
      </div>
    </div>
  )
}
