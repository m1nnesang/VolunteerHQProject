import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getFundraisers } from '../api/fundraisers'
import FundraiserCard from '../components/FundraiserCard'
import { useAuth } from '../context/AuthContext'

export default function FundraisersPage() {
  const { isUnit } = useAuth()
  const [fundraisers, setFundraisers] = useState([])
  const [total, setTotal] = useState(0)
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const pageSize = 12

  useEffect(() => {
    setLoading(true)
    getFundraisers(page, pageSize)
      .then(res => {
        setFundraisers(res.data?.items || [])
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
            <h1 className="text-white text-3xl font-bold mb-2">Збори</h1>
            <p className="text-gray-500">Всього зборів: {total}</p>
          </div>
          {isUnit && (
            <Link
              to="/fundraisers/create"
              className="bg-yellow-400 text-black font-semibold px-5 py-2.5 rounded-lg hover:bg-yellow-300 transition-colors text-sm"
            >
              + Створити збір
            </Link>
          )}
        </div>

        {loading ? (
          <div className="grid md:grid-cols-3 gap-4">
            {Array.from({ length: 6 }).map((_, i) => (
              <div key={i} className="bg-[#1c1d24] rounded-xl p-5 animate-pulse h-40" />
            ))}
          </div>
        ) : fundraisers.length === 0 ? (
          <div className="text-center py-20 text-gray-500">
            Зборів поки немає
          </div>
        ) : (
          <div className="grid md:grid-cols-3 gap-4">
            {fundraisers.map(f => <FundraiserCard key={f.id} f={f} />)}
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
            <span className="px-4 py-2 text-gray-400 text-sm">
              {page} / {totalPages}
            </span>
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
