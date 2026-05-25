import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getStats } from '../api/stats'
import { getFundraisers } from '../api/fundraisers'
import FundraiserCard from '../components/FundraiserCard'
import { useAuth } from '../context/AuthContext'

export default function HomePage() {
  const { user } = useAuth()
  const [stats, setStats] = useState(null)
  const [fundraisers, setFundraisers] = useState([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    Promise.all([getStats(), getFundraisers(1, 6)])
      .then(([statsRes, fundraisersRes]) => {
        setStats(statsRes.data)
        setFundraisers(fundraisersRes.data?.items || [])
        // totalCount используется только на странице списка
      })
      .catch(console.error)
      .finally(() => setLoading(false))
  }, [])

  return (
    <div className="min-h-screen bg-[#111218]">
      {/* Hero */}
      <section className="max-w-6xl mx-auto px-4 pt-20 pb-16">
        <div className="grid md:grid-cols-2 gap-12 items-center">
          <div>
            <div className="inline-flex items-center gap-2 bg-yellow-400/10 border border-yellow-400/20 rounded-full px-3 py-1 mb-6">
              <span className="w-1.5 h-1.5 bg-yellow-400 rounded-full"></span>
              <span className="text-yellow-400 text-xs font-medium">ВОЛОНТЕРСЬКА ПЛАТФОРМА</span>
            </div>
            <h1 className="text-5xl font-bold text-white leading-tight mb-2">
              Допомога —
            </h1>
            <h1 className="text-5xl font-bold text-yellow-400 leading-tight mb-6">
              прозоро<br />та ефективно
            </h1>
            <p className="text-gray-400 text-base leading-relaxed mb-8 max-w-md">
              VolunteerHQ об'єднує волонтерів, організації та підрозділи ЗСУ в єдину систему.
              Від збору до підтвердження — кожна гривня на рахунку.
            </p>
            <div className="flex gap-4">
              <Link
                to="/fundraisers"
                className="bg-yellow-400 text-black font-semibold px-6 py-3 rounded-lg hover:bg-yellow-300 transition-colors"
              >
                Переглянути збори →
              </Link>
              {!user && (
                <Link
                  to="/register"
                  className="border border-[#2e303a] text-white px-6 py-3 rounded-lg hover:border-gray-500 transition-colors"
                >
                  Зареєструватись
                </Link>
              )}
            </div>
          </div>

          {/* Active fundraisers preview */}
          <div className="bg-[#1c1d24] border border-[#2e303a] rounded-2xl p-5">
            <div className="flex items-center gap-2 mb-4">
              <span className="w-2 h-2 bg-green-400 rounded-full animate-pulse"></span>
              <span className="text-gray-400 text-xs font-medium tracking-wider">АКТИВНІ ЗБОРИ</span>
            </div>
            <div className="space-y-3">
              {loading ? (
                [1, 2, 3].map(i => (
                  <div key={i} className="bg-[#2e303a] rounded-lg p-3 animate-pulse h-14" />
                ))
              ) : fundraisers.slice(0, 3).map(f => (
                <Link
                  key={f.id}
                  to={`/fundraisers/${f.id}`}
                  className="flex items-center gap-3 bg-[#111218] rounded-lg p-3 hover:bg-[#0d0e14] transition-colors block"
                >
                  <div className="w-8 h-8 bg-yellow-400/20 rounded-lg flex items-center justify-center text-yellow-400 text-xs font-bold shrink-0">
                    {f.title?.slice(0, 2).toUpperCase()}
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-white text-sm font-medium truncate">{f.title}</p>
                    <div className="w-full bg-[#2e303a] rounded-full h-1 mt-1">
                      <div
                        className="bg-yellow-400 h-1 rounded-full"
                        style={{
                          width: `${Math.min(100, f.totalGoal > 0 ? (f.currentProgress / f.totalGoal) * 100 : 0)}%`
                        }}
                      />
                    </div>
                  </div>
                </Link>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* Stats */}
      <section className="border-t border-[#2e303a]">
        <div className="max-w-6xl mx-auto px-4 py-12">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
            {[
              { label: 'ЗІБРАНО', value: stats ? `${(stats.totalDonated / 1_000_000).toFixed(1)}М+ ₴` : '—' },
              { label: 'ЗБОРІВ', value: stats ? stats.activeFundraisers + stats.completedFundraisers : '—' },
              { label: 'АКТИВНИХ ЗБОРІВ', value: stats ? stats.activeFundraisers : '—' },
              { label: 'ЗАВЕРШЕНИХ', value: stats ? stats.completedFundraisers : '—' },
            ].map(({ label, value }) => (
              <div key={label}>
                <div className="text-3xl font-bold text-white mb-1">{value}</div>
                <div className="text-gray-500 text-xs font-medium tracking-wider">{label}</div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Features */}
      <section className="border-t border-[#2e303a]">
        <div className="max-w-6xl mx-auto px-4 py-12">
          <div className="grid grid-cols-2 md:grid-cols-3 gap-6 mb-12">
            {[
              { icon: '💳', title: 'Прозорі платежі' },
              { icon: '🎖', title: 'Прямий зв\'язок з ЗСУ' },
              { icon: '📊', title: 'Аналітика в реальному часі' },
              { icon: '🔔', title: 'Підписки та сповіщення' },
              { icon: '🛡', title: 'Верифіковані організації' },
              { icon: '📋', title: 'Звітність та AuditLog' },
            ].map(({ icon, title }) => (
              <div key={title} className="bg-[#1c1d24] border border-[#2e303a] rounded-xl p-5 flex items-center gap-3">
                <span className="text-2xl">{icon}</span>
                <span className="text-white text-sm font-medium">{title}</span>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Recent fundraisers */}
      <section className="border-t border-[#2e303a]">
        <div className="max-w-6xl mx-auto px-4 py-12">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-white text-xl font-bold">Актуальні збори</h2>
            <Link to="/fundraisers" className="text-yellow-400 text-sm hover:text-yellow-300 transition-colors">
              Всі збори →
            </Link>
          </div>
          {loading ? (
            <div className="grid md:grid-cols-3 gap-4">
              {[1, 2, 3].map(i => (
                <div key={i} className="bg-[#1c1d24] rounded-xl p-5 animate-pulse h-40" />
              ))}
            </div>
          ) : (
            <div className="grid md:grid-cols-3 gap-4">
              {fundraisers.map(f => <FundraiserCard key={f.id} f={f} />)}
            </div>
          )}
        </div>
      </section>

      {/* Footer */}
      <footer className="border-t border-[#2e303a] mt-auto">
        <div className="max-w-6xl mx-auto px-4 py-6 flex items-center justify-between text-gray-500 text-sm">
          <span>VolunteerHQ</span>
          <span>© 2025 · Зроблено в Україні 🇺🇦</span>
        </div>
      </footer>
    </div>
  )
}
