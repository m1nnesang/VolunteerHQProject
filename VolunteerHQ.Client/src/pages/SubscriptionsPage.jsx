import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { getSubscriptions, unsubscribe } from '../api/subscriptions'
import { useAuth } from '../context/AuthContext'

const targetMeta = {
  Organization: {
    label: 'Організація',
    icon: '🏢',
    color: 'bg-yellow-400/20 text-yellow-400',
    path: 'organizations',
  },
  MilitaryUnit: {
    label: 'Військовий підрозділ',
    icon: '🎖',
    color: 'bg-green-400/20 text-green-400',
    path: 'units',
  },
}

export default function SubscriptionsPage() {
  const { user } = useAuth()
  const navigate = useNavigate()
  const [subscriptions, setSubscriptions] = useState([])
  const [loading, setLoading] = useState(true)

  const load = () => {
    setLoading(true)
    getSubscriptions(1, 100)
      .then(res => setSubscriptions(res.data?.items || []))
      .catch(console.error)
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    if (!user) {
      navigate('/login')
      return
    }
    load()
  }, [])

  const handleUnsubscribe = async (subscriptionId) => {
    try {
      await unsubscribe(subscriptionId)
      setSubscriptions(prev => prev.filter(s => s.id !== subscriptionId))
    } catch (err) {
      console.error(err)
    }
  }

  return (
    <div className="min-h-screen bg-[#111218]">
      <div className="max-w-2xl mx-auto px-4 py-10">
        <div className="mb-6">
          <h1 className="text-white text-3xl font-bold mb-2">Мої підписки</h1>
          <p className="text-gray-500 text-sm">
            Отримуйте сповіщення про новини організацій та підрозділів
          </p>
        </div>

        {loading ? (
          <div className="space-y-3">
            {[1, 2, 3].map(i => (
              <div key={i} className="bg-[#1c1d24] rounded-xl h-20 animate-pulse" />
            ))}
          </div>
        ) : subscriptions.length === 0 ? (
          <div className="bg-[#1c1d24] border border-[#2e303a] rounded-2xl p-12 text-center">
            <div className="text-4xl mb-3">🔔</div>
            <p className="text-gray-400 mb-4">У вас ще немає підписок</p>
            <div className="flex gap-3 justify-center">
              <Link
                to="/organizations"
                className="text-yellow-400 hover:text-yellow-300 text-sm"
              >
                Організації
              </Link>
              <span className="text-gray-700">·</span>
              <Link
                to="/units"
                className="text-yellow-400 hover:text-yellow-300 text-sm"
              >
                Підрозділи
              </Link>
            </div>
          </div>
        ) : (
          <div className="space-y-3">
            {subscriptions.map(sub => {
              const meta = targetMeta[sub.target] || { label: sub.target, icon: '?', color: '', path: '' }
              return (
                <div
                  key={sub.id}
                  className="bg-[#1c1d24] border border-[#2e303a] rounded-xl p-4 flex items-center gap-4"
                >
                  <div className={`w-12 h-12 rounded-xl flex items-center justify-center text-xl shrink-0 ${meta.color}`}>
                    {meta.icon}
                  </div>
                  <Link
                    to={`/${meta.path}/${sub.targetId}`}
                    className="flex-1 min-w-0 hover:opacity-80 transition-opacity"
                  >
                    <div className="text-white font-medium truncate">
                      {sub.targetName || `${meta.label} #${sub.targetId}`}
                    </div>
                    <div className="text-xs text-gray-600 mt-1">
                      {meta.label} · підписано {new Date(sub.subscribedAt).toLocaleDateString('uk-UA')}
                    </div>
                  </Link>
                  <button
                    onClick={() => handleUnsubscribe(sub.id)}
                    className="text-sm border border-[#2e303a] text-gray-500 px-3 py-1.5 rounded-lg hover:border-red-400/50 hover:text-red-400 transition-colors shrink-0"
                  >
                    Відписатись
                  </button>
                </div>
              )
            })}
          </div>
        )}
      </div>
    </div>
  )
}
