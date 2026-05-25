import { Link } from 'react-router-dom'
import { formatUah } from '../utils/money'

const statusLabel = {
  Open: { text: 'Відкрито', color: 'text-green-400 bg-green-400/10' },
  InProgress: { text: 'Збираємо', color: 'text-blue-400 bg-blue-400/10' },
  Completed: { text: 'Виконано', color: 'text-yellow-400 bg-yellow-400/10' },
  Cancelled: { text: 'Скасовано', color: 'text-red-400 bg-red-400/10' },
  Closed: { text: 'Закрито', color: 'text-gray-400 bg-gray-400/10' },
}

export default function FundraiserCard({ f }) {
  const percent = f.totalGoal > 0
    ? Math.min(100, Math.round((f.currentProgress / f.totalGoal) * 100))
    : 0

  const status = statusLabel[f.status] || { text: f.status, color: 'text-gray-400 bg-gray-400/10' }

  return (
    <Link
      to={`/fundraisers/${f.id}`}
      className="bg-[#1c1d24] border border-[#2e303a] rounded-xl p-5 hover:border-gray-500 transition-all block"
    >
      <div className="flex items-start justify-between mb-3">
        <h3 className="text-white font-semibold text-sm leading-snug line-clamp-2 flex-1 pr-2">
          {f.title}
        </h3>
        <span className={`text-xs px-2 py-0.5 rounded-full font-medium shrink-0 ${status.color}`}>
          {status.text}
        </span>
      </div>

      <p className="text-gray-500 text-xs mb-4 line-clamp-2">{f.description}</p>

      <div className="space-y-2">
        <div className="flex justify-between text-xs text-gray-400">
          <span>{formatUah(f.currentProgress)}</span>
          <span>{formatUah(f.totalGoal)}</span>
        </div>
        <div className="w-full bg-[#2e303a] rounded-full h-1.5">
          <div
            className="bg-yellow-400 h-1.5 rounded-full transition-all"
            style={{ width: `${percent}%` }}
          />
        </div>
        <div className="text-xs text-gray-500 text-right">{percent}%</div>
      </div>
    </Link>
  )
}
