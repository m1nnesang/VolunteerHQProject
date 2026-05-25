import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getOrganizations } from '../api/organizations'
import { useAuth } from '../context/AuthContext'

export default function OrganizationsPage() {
  const { isUser } = useAuth()
  const [organizations, setOrganizations] = useState([])
  const [total, setTotal] = useState(0)
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const pageSize = 12

  useEffect(() => {
    setLoading(true)
    getOrganizations(page, pageSize)
      .then(res => {
        setOrganizations(res.data?.items || [])
        setTotal(res.data?.totalCount || 0)
      })
      .catch(console.error)
      .finally(() => setLoading(false))
  }, [page])

  const totalPages = Math.ceil(total / pageSize)

  return (
    <div className="min-h-screen bg-[#111218]">
      <div className="max-w-6xl mx-auto px-4 py-10">
        <div className="flex items-center justify-between mb-8">
          <div>
            <h1 className="text-white text-3xl font-bold mb-2">Організації</h1>
            <p className="text-gray-500">Всього організацій: {total}</p>
          </div>
          {isUser && (
            <Link
              to="/organizations/create"
              className="bg-yellow-400 text-black font-semibold px-5 py-2.5 rounded-lg hover:bg-yellow-300 transition-colors text-sm"
            >
              + Створити організацію
            </Link>
          )}
        </div>

        {loading ? (
          <div className="grid md:grid-cols-3 gap-4">
            {Array.from({ length: 6 }).map((_, i) => (
              <div key={i} className="bg-[#1c1d24] rounded-xl p-6 animate-pulse h-40" />
            ))}
          </div>
        ) : organizations.length === 0 ? (
          <div className="text-center py-20 text-gray-500">Організацій поки немає</div>
        ) : (
          <div className="grid md:grid-cols-3 gap-4">
            {organizations.map(org => (
              <Link
                key={org.id}
                to={`/organizations/${org.id}`}
                className="bg-[#1c1d24] border border-[#2e303a] rounded-xl p-6 hover:border-gray-500 transition-all block"
              >
                <div className="flex items-center gap-4 mb-4">
                  <div className="w-12 h-12 bg-yellow-400/20 rounded-xl flex items-center justify-center text-yellow-400 text-lg font-bold shrink-0">
                    {org.name?.charAt(0).toUpperCase()}
                  </div>
                  <div className="min-w-0">
                    <h3 className="text-white font-semibold truncate">{org.name}</h3>
                    <p className="text-gray-500 text-sm">{org.city}</p>
                  </div>
                </div>
                <p className="text-gray-400 text-sm line-clamp-2">{org.description}</p>
                <div className="mt-4 text-xs text-gray-600">
                  З {new Date(org.createdAt).toLocaleDateString('uk-UA', { month: 'long', year: 'numeric' })}
                </div>
              </Link>
            ))}
          </div>
        )}

        {totalPages > 1 && (
          <div className="flex justify-center gap-2 mt-8">
            <button
              onClick={() => setPage(p => Math.max(1, p - 1))}
              disabled={page === 1}
              className="px-4 py-2 border border-[#2e303a] text-gray-400 rounded-lg hover:border-gray-500 disabled:opacity-30 disabled:cursor-not-allowed text-sm transition-colors"
            >
              ← Назад
            </button>
            <span className="px-4 py-2 text-gray-400 text-sm">{page} / {totalPages}</span>
            <button
              onClick={() => setPage(p => Math.min(totalPages, p + 1))}
              disabled={page === totalPages}
              className="px-4 py-2 border border-[#2e303a] text-gray-400 rounded-lg hover:border-gray-500 disabled:opacity-30 disabled:cursor-not-allowed text-sm transition-colors"
            >
              Далі →
            </button>
          </div>
        )}
      </div>
    </div>
  )
}
