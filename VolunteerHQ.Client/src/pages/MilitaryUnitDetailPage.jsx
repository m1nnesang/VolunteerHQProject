import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { getMilitaryUnit } from '../api/militaryUnits'
import SubscribeButton from '../components/SubscribeButton'

export default function MilitaryUnitDetailPage() {
  const { id } = useParams()
  const [unit, setUnit] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    getMilitaryUnit(id)
      .then(res => setUnit(res.data))
      .catch(console.error)
      .finally(() => setLoading(false))
  }, [id])

  if (loading) {
    return (
      <div className="min-h-screen bg-[#111218] flex items-center justify-center">
        <div className="text-gray-500">Завантаження...</div>
      </div>
    )
  }

  if (!unit) {
    return (
      <div className="min-h-screen bg-[#111218] flex items-center justify-center">
        <div className="text-gray-500">Підрозділ не знайдено</div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-[#111218]">
      <div className="max-w-3xl mx-auto px-4 py-10">
        <div className="bg-[#1c1d24] border border-[#2e303a] rounded-2xl p-8">
          <div className="flex items-start justify-between gap-4 mb-6">
            <div className="flex items-center gap-5 min-w-0">
              <div className="w-20 h-20 bg-green-400/20 rounded-2xl flex items-center justify-center text-green-400 text-3xl shrink-0">
                🎖
              </div>
              <div className="min-w-0">
                <h1 className="text-white text-2xl font-bold">
                  {unit.unitName || '********'}
                </h1>
                <p className="text-gray-500 text-sm mt-1">Військовий підрозділ ЗСУ</p>
              </div>
            </div>
            <SubscribeButton target="MilitaryUnit" targetId={parseInt(id)} />
          </div>

          <div className="space-y-3 pt-4 border-t border-[#2e303a]">
            <div className="flex justify-between text-sm">
              <span className="text-gray-500">Контактна особа</span>
              <span className="text-white">{unit.contactPersonName || '********'}</span>
            </div>
            <div className="flex justify-between text-sm">
              <span className="text-gray-500">Створено</span>
              <span className="text-white">
                {new Date(unit.createdAt).toLocaleDateString('uk-UA', {
                  day: 'numeric', month: 'long', year: 'numeric'
                })}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}
