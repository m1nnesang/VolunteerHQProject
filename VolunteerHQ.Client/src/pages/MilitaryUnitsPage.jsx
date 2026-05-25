import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getMilitaryUnits } from '../api/militaryUnits'

export default function MilitaryUnitsPage() {
  const [units, setUnits] = useState([])
  const [total, setTotal] = useState(0)
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const pageSize = 12

  useEffect(() => {
    setLoading(true)
    getMilitaryUnits(page, pageSize)
      .then(res => {
        setUnits(res.data?.items || [])
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
            <h1 className="text-white text-3xl font-bold mb-2">Військові підрозділи</h1>
            <p className="text-gray-500">Всього підрозділів: {total}</p>
          </div>
        </div>

        {loading ? (
          <div className="grid md:grid-cols-3 gap-4">
            {Array.from({ length: 6 }).map((_, i) => (
              <div key={i} className="bg-[#1c1d24] rounded-xl p-6 animate-pulse h-32" />
            ))}
          </div>
        ) : units.length === 0 ? (
          <div className="text-center py-20 text-gray-500">Підрозділів поки немає</div>
        ) : (
          <div className="grid md:grid-cols-3 gap-4">
            {units.map(unit => (
              <Link
                key={unit.id}
                to={`/units/${unit.id}`}
                className="bg-[#1c1d24] border border-[#2e303a] rounded-xl p-6 hover:border-gray-500 transition-all block"
              >
                <div className="flex items-center gap-4 mb-3">
                  <div className="w-12 h-12 bg-green-400/20 rounded-xl flex items-center justify-center text-green-400 text-lg font-bold shrink-0">
                    🎖
                  </div>
                  <div className="min-w-0">
                    <h3 className="text-white font-semibold truncate">
                      {unit.unitName || '********'}
                    </h3>
                    <p className="text-gray-500 text-xs">Військовий підрозділ</p>
                  </div>
                </div>
                <div className="text-xs text-gray-600 mt-3">
                  З {new Date(unit.createdAt).toLocaleDateString('uk-UA', { month: 'long', year: 'numeric' })}
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
