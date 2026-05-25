import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import toast from 'react-hot-toast'
import { getFundraiser, directDonate } from '../api/fundraisers'
import { useAuth } from '../context/AuthContext'
import Comments from '../components/Comments'
import ReportButton from '../components/ReportButton'
import AssignOrgPanel from '../components/AssignOrgPanel'
import { formatUah, formatMoney, unformatMoney } from '../utils/money'

function AssignmentRow({ assignment, link, isMember }) {
  const [copied, setCopied] = useState(false)

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(link)
      setCopied(true)
      setTimeout(() => setCopied(false), 2000)
    } catch {
      // fallback
      const ta = document.createElement('textarea')
      ta.value = link
      document.body.appendChild(ta)
      ta.select()
      document.execCommand('copy')
      document.body.removeChild(ta)
      setCopied(true)
      setTimeout(() => setCopied(false), 2000)
    }
  }

  return (
    <div className="bg-[#111218] rounded-lg p-3">
      <div className="flex items-center justify-between mb-2">
        <div className="text-sm">
          <div className="text-gray-300">{assignment.organizationName || `Організація #${assignment.organizationId}`}</div>
          <div className="text-gray-600 text-xs">
            Зібрано {formatUah(assignment.amountRaised ?? 0)}
          </div>
        </div>
        {isMember && (
          <button
            onClick={handleCopy}
            className={`text-xs px-3 py-1.5 rounded-lg border transition-colors ${
              copied
                ? 'bg-green-400/10 border-green-400/30 text-green-400'
                : 'border-[#2e303a] text-gray-400 hover:border-yellow-400 hover:text-yellow-400'
            }`}
          >
            {copied ? '✓ Скопійовано' : '📋 Копіювати посилання'}
          </button>
        )}
      </div>
      {isMember && (
        <input
          type="text"
          value={link}
          readOnly
          onClick={e => e.target.select()}
          className="w-full bg-[#1c1d24] border border-[#2e303a] rounded text-xs text-gray-500 px-2 py-1.5 font-mono cursor-text focus:outline-none focus:border-yellow-400"
        />
      )}
    </div>
  )
}

const statusLabel = {
  Open: { text: 'Відкрито', color: 'text-green-400 bg-green-400/10 border-green-400/20' },
  InProgress: { text: 'Збираємо', color: 'text-blue-400 bg-blue-400/10 border-blue-400/20' },
  Completed: { text: 'Виконано', color: 'text-yellow-400 bg-yellow-400/10 border-yellow-400/20' },
  Cancelled: { text: 'Скасовано', color: 'text-red-400 bg-red-400/10 border-red-400/20' },
  Closed: { text: 'Закрито', color: 'text-gray-400 bg-gray-400/10 border-gray-400/20' },
}

export default function FundraiserDetailPage() {
  const { id } = useParams()
  const { user, managedOrgId, isUnit } = useAuth()
  const [fundraiser, setFundraiser] = useState(null)
  const [loading, setLoading] = useState(true)
  const [donateAmount, setDonateAmount] = useState('')
  const [donateLoading, setDonateLoading] = useState(false)

  const load = () => {
    setLoading(true)
    getFundraiser(id)
      .then(res => setFundraiser(res.data))
      .catch(console.error)
      .finally(() => setLoading(false))
  }

  useEffect(() => { load() }, [id])

  const handleDonate = async (e) => {
    e.preventDefault()
    const amount = parseFloat(donateAmount)
    if (!amount || amount <= 0) {
      toast.error('Введіть коректну суму')
      return
    }
    setDonateLoading(true)
    try {
      await directDonate(id, { amount })
      toast.success('Дякуємо за донат! 💛')
      setDonateAmount('')
      load()
    } catch (err) {
      toast.error(err.response?.data?.error || 'Помилка при донаті')
    } finally {
      setDonateLoading(false)
    }
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-[#111218] flex items-center justify-center">
        <div className="text-gray-500">Завантаження...</div>
      </div>
    )
  }

  if (!fundraiser) {
    return (
      <div className="min-h-screen bg-[#111218] flex items-center justify-center">
        <div className="text-gray-500">Збір не знайдено</div>
      </div>
    )
  }

  const percent = fundraiser.totalGoal > 0
    ? Math.min(100, Math.round((fundraiser.currentProgress / fundraiser.totalGoal) * 100))
    : 0

  const status = statusLabel[fundraiser.status] || { text: fundraiser.status, color: 'text-gray-400 bg-gray-400/10 border-gray-400/20' }
  const canDonate = !isUnit && (fundraiser.status === 'Open' || fundraiser.status === 'InProgress')

  return (
    <div className="min-h-screen bg-[#111218]">
      <div className="max-w-4xl mx-auto px-4 py-10">
        <div className="grid md:grid-cols-3 gap-8">
          {/* Main info */}
          <div className="md:col-span-2 space-y-6">
            <div>
              <span className={`inline-flex items-center text-xs px-2.5 py-1 rounded-full border font-medium mb-4 ${status.color}`}>
                {status.text}
              </span>
              <h1 className="text-white text-2xl font-bold mb-3">{fundraiser.title}</h1>
              <p className="text-gray-400 leading-relaxed">{fundraiser.description}</p>
              <div className="mt-3">
                <ReportButton reportedId={parseInt(id)} label="Поскаржитись на збір" />
              </div>
            </div>

            {/* Progress */}
            <div className="bg-[#1c1d24] border border-[#2e303a] rounded-xl p-6">
              <div className="flex justify-between items-end mb-3">
                <div>
                  <div className="text-2xl font-bold text-white">
                    {formatUah(fundraiser.currentProgress)}
                  </div>
                  <div className="text-gray-500 text-sm">зібрано з {formatUah(fundraiser.totalGoal)}</div>
                </div>
                <div className="text-3xl font-bold text-yellow-400">{percent}%</div>
              </div>
              <div className="w-full bg-[#2e303a] rounded-full h-3">
                <div
                  className="bg-yellow-400 h-3 rounded-full transition-all duration-500"
                  style={{ width: `${percent}%` }}
                />
              </div>
            </div>

            {/* Assignments */}
            {fundraiser.assignments && fundraiser.assignments.length > 0 && (
              <div className="bg-[#1c1d24] border border-[#2e303a] rounded-xl p-5">
                <h3 className="text-white font-semibold mb-3 text-sm">Підключені організації</h3>
                <div className="space-y-3">
                  {fundraiser.assignments.map(a => {
                    const code = a.uniqueCode || a.uniqueId
                    const link = `${window.location.origin}/donate/${code}?fundraiserId=${id}`
                    return (
                      <AssignmentRow key={a.id} assignment={a} link={link} isMember={managedOrgId === a.organizationId} />
                    )
                  })}
                </div>
              </div>
            )}

            {/* Assign panel */}
            {(fundraiser.status === 'Open' || fundraiser.status === 'InProgress') && (
              <AssignOrgPanel fundraiserId={parseInt(id)} onAssigned={load} />
            )}

            {/* Deadline */}
            {fundraiser.deadline && (
              <div className="bg-[#1c1d24] border border-[#2e303a] rounded-xl p-4">
                <div className="text-gray-500 text-sm">Дедлайн</div>
                <div className="text-white font-medium">
                  {new Date(fundraiser.deadline).toLocaleDateString('uk-UA', {
                    day: 'numeric', month: 'long', year: 'numeric'
                  })}
                </div>
              </div>
            )}

            {/* Коментарі */}
            <Comments fundraiserId={parseInt(id)} />
          </div>

          {/* Donate panel */}
          {!isUnit && <div>
            <div className="bg-[#1c1d24] border border-[#2e303a] rounded-xl p-6 sticky top-20">
              <h3 className="text-white font-semibold mb-4">Підтримати збір</h3>

              {canDonate ? (
                <form onSubmit={handleDonate} className="space-y-4">
                  <div className="grid grid-cols-3 gap-2">
                    {[100, 500, 1000].map(amount => (
                      <button
                        key={amount}
                        type="button"
                        onClick={() => setDonateAmount(String(amount))}
                        className={`py-2 text-sm rounded-lg border transition-colors ${
                          donateAmount === String(amount)
                            ? 'bg-yellow-400 text-black border-yellow-400 font-semibold'
                            : 'border-[#2e303a] text-gray-400 hover:border-gray-500'
                        }`}
                      >
                        {amount}&nbsp;₴
                      </button>
                    ))}
                  </div>
                  <div className="relative">
                    <input
                      type="text"
                      inputMode="numeric"
                      value={formatMoney(donateAmount)}
                      onChange={e => setDonateAmount(unformatMoney(e.target.value))}
                      placeholder="Своя сума"
                      className="w-full bg-[#111218] border border-[#2e303a] rounded-lg px-4 py-3 pr-9 text-white text-sm placeholder-gray-600 focus:outline-none focus:border-yellow-400 transition-colors"
                    />
                    <span className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 text-sm pointer-events-none">₴</span>
                  </div>
                  <button
                    type="submit"
                    disabled={donateLoading}
                    className="w-full bg-yellow-400 text-black font-semibold py-3 rounded-lg hover:bg-yellow-300 transition-colors disabled:opacity-50"
                  >
                    {donateLoading ? 'Обробка...' : '💛 Задонатити'}
                  </button>
                  {!user && (
                    <p className="text-gray-600 text-xs text-center">
                      Анонімний донат. Увійдіть щоб відстежувати внески.
                    </p>
                  )}
                </form>
              ) : (
                <div className="text-gray-500 text-sm text-center py-4">
                  Цей збір більше не приймає донатів
                </div>
              )}
            </div>
          </div>}
        </div>
      </div>
    </div>
  )
}
