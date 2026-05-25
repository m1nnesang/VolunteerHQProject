import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { getMe, getMyStats } from '../api/user'
import { getJoinRequest } from '../api/joinRequests'
import { useAuth } from '../context/AuthContext'
import { formatUah } from '../utils/money'

const roleLabel = {
  User: 'Користувач',
  Volunteer: 'Волонтер',
  Admin: 'Адміністратор',
}

export default function ProfilePage() {
  const { user, logoutUser, initials } = useAuth()
  const navigate = useNavigate()
  const [profile, setProfile] = useState(null)
  const [stats, setStats] = useState(null)
  const [joinRequest, setJoinRequest] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    Promise.all([getMe(), getMyStats()])
      .then(([meRes, statsRes]) => {
        setProfile(meRes.data)
        setStats(statsRes.data)
      })
      .catch(console.error)
      .finally(() => setLoading(false))

    // Перевіряємо чи є збережена заявка
    const saved = localStorage.getItem('myJoinRequest')
    if (saved) {
      try {
        const { requestId, orgId, orgName } = JSON.parse(saved)
        getJoinRequest(requestId, orgId)
          .then(res => setJoinRequest({ ...res.data, orgName }))
          .catch(() => {
            // Заявка більше не доступна — видаляємо
            localStorage.removeItem('myJoinRequest')
          })
      } catch {
        localStorage.removeItem('myJoinRequest')
      }
    }
  }, [])

  const handleLogout = () => {
    logoutUser()
    navigate('/')
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-[#111218] flex items-center justify-center">
        <div className="text-gray-500">Завантаження...</div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-[#111218]">
      <div className="max-w-2xl mx-auto px-4 py-10">

        {/* Аватар + ім'я */}
        <div className="bg-[#1c1d24] border border-[#2e303a] rounded-2xl p-8 mb-6 flex items-center gap-6">
          <div className="w-20 h-20 bg-yellow-400 rounded-full flex items-center justify-center text-black text-2xl font-bold shrink-0">
            {initials?.toUpperCase() || '?'}
          </div>
          <div>
            <h1 className="text-white text-2xl font-bold">
              {profile?.firstName} {profile?.secondName}
            </h1>
            <p className="text-gray-500 text-sm mt-1">{profile?.email}</p>
            <span className="inline-block mt-2 text-xs px-2.5 py-1 rounded-full bg-yellow-400/10 text-yellow-400 border border-yellow-400/20 font-medium">
              {roleLabel[profile?.role] || profile?.role}
            </span>
          </div>
        </div>

        {/* Статистика */}
        <div className="grid grid-cols-3 gap-4 mb-6">
          {[
            { label: 'Задоначено', value: formatUah(stats?.totalDonated ?? 0) },
            { label: 'Донатів', value: stats?.donationsCount ?? 0 },
            { label: 'Зборів підтримано', value: stats?.fundraisersSupported ?? 0 },
          ].map(({ label, value }) => (
            <div key={label} className="bg-[#1c1d24] border border-[#2e303a] rounded-xl p-5 text-center">
              <div className="text-2xl font-bold text-white mb-1">{value}</div>
              <div className="text-gray-500 text-xs">{label}</div>
            </div>
          ))}
        </div>

        {/* Моя заявка */}
        {joinRequest && (
          <div className="bg-[#1c1d24] border border-[#2e303a] rounded-2xl p-6 mb-6">
            <h2 className="text-white font-semibold mb-4">Моя заявка</h2>
            <Link
              to={`/organizations/${joinRequest.organizationId}`}
              className="flex items-center justify-between bg-[#111218] rounded-xl p-4 hover:bg-[#0d0e14] transition-colors"
            >
              <div>
                <div className="text-white text-sm font-medium">
                  {joinRequest.orgName || `Організація #${joinRequest.organizationId}`}
                </div>
                <div className="text-gray-500 text-xs mt-1">
                  Подано {new Date(joinRequest.createdAt).toLocaleDateString('uk-UA')}
                </div>
              </div>
              <span className={`text-xs px-3 py-1 rounded-full font-medium ${
                joinRequest.status === 'Pending' ? 'bg-yellow-400/10 text-yellow-400 border border-yellow-400/20' :
                joinRequest.status === 'Approved' ? 'bg-green-400/10 text-green-400 border border-green-400/20' :
                'bg-red-400/10 text-red-400 border border-red-400/20'
              }`}>
                {joinRequest.status === 'Pending' ? 'На розгляді' :
                 joinRequest.status === 'Approved' ? 'Схвалено' :
                 'Відхилено'}
              </span>
            </Link>
          </div>
        )}

        {/* Інфо */}
        <div className="bg-[#1c1d24] border border-[#2e303a] rounded-2xl p-6 mb-6">
          <h2 className="text-white font-semibold mb-4">Інформація</h2>
          <div className="space-y-3">
            <div className="flex justify-between text-sm">
              <span className="text-gray-500">Email</span>
              <span className="text-white">{profile?.email}</span>
            </div>
            {profile?.birthDate && (
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">Дата народження</span>
                <span className="text-white">
                  {new Date(profile.birthDate).toLocaleDateString('uk-UA', {
                    day: 'numeric', month: 'long', year: 'numeric'
                  })}
                </span>
              </div>
            )}
          </div>
        </div>

        <Link
          to="/subscriptions"
          className="block bg-[#1c1d24] border border-[#2e303a] rounded-xl p-4 mb-4 hover:border-gray-500 transition-colors"
        >
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <span className="text-2xl">🔔</span>
              <div>
                <div className="text-white font-medium text-sm">Мої підписки</div>
                <div className="text-gray-500 text-xs">Організації та підрозділи</div>
              </div>
            </div>
            <span className="text-gray-500">→</span>
          </div>
        </Link>

        <button
          onClick={handleLogout}
          className="w-full border border-red-500/30 text-red-400 py-3 rounded-lg hover:bg-red-500/10 transition-colors text-sm font-medium"
        >
          Вийти з акаунту
        </button>
      </div>
    </div>
  )
}
