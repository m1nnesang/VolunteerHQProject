import { useEffect, useState } from 'react'
import { useParams, useSearchParams, Link } from 'react-router-dom'
import { donateViaCode, getFundraiser } from '../api/fundraisers'
import { getOrganization } from '../api/organizations'
import { useAuth } from '../context/AuthContext'
import { formatMoney, unformatMoney } from '../utils/money'

export default function DonateViaCodePage() {
  const { uniqueCode } = useParams()
  const [searchParams] = useSearchParams()
  const fundraiserId = searchParams.get('fundraiserId')
  const { user } = useAuth()

  const [fundraiser, setFundraiser] = useState(null)
  const [orgName, setOrgName] = useState(null)
  const [loading, setLoading] = useState(true)
  const [amount, setAmount] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [success, setSuccess] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!fundraiserId) {
      setLoading(false)
      return
    }
    getFundraiser(fundraiserId)
      .then(res => {
        const data = res.data
        setFundraiser(data)
        const assignment = data.assignments?.find(a => a.uniqueId === uniqueCode)
        if (assignment?.organizationId) {
          return getOrganization(assignment.organizationId)
            .then(orgRes => setOrgName(orgRes.data?.name))
            .catch(() => {})
        }
      })
      .catch(console.error)
      .finally(() => setLoading(false))
  }, [fundraiserId])

  const handleDonate = async (e) => {
    e.preventDefault()
    const value = parseFloat(amount)
    if (!value || value <= 0) {
      setError('Введіть суму')
      return
    }
    setError('')
    setSubmitting(true)
    try {
      await donateViaCode(fundraiserId, uniqueCode, { amount: value })
      setSuccess(true)
      setAmount('')
    } catch (err) {
      setError(err.response?.data?.error || 'Помилка при донаті')
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-[#111218] flex items-center justify-center">
        <div className="text-gray-500">Завантаження...</div>
      </div>
    )
  }

  if (!fundraiserId) {
    return (
      <div className="min-h-screen bg-[#111218] flex items-center justify-center px-4">
        <div className="text-center">
          <p className="text-gray-400 mb-2">Невірне посилання</p>
          <p className="text-gray-600 text-sm">Не вказано ID збору</p>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-[#111218] flex items-center justify-center px-4 py-10">
      <div className="w-full max-w-md">
        <div className="text-center mb-6">
          <Link to="/" className="text-white font-bold text-2xl">VolunteerHQ</Link>
          <p className="text-gray-500 text-sm mt-2">Донат через унікальний код організації</p>
        </div>

        <div className="bg-[#1c1d24] border border-[#2e303a] rounded-2xl p-8">
          {fundraiser && (
            <div className="mb-6 pb-6 border-b border-[#2e303a]">
              <div className="text-yellow-400 text-xs font-medium mb-2">ЗБІР</div>
              <h2 className="text-white text-lg font-semibold">{fundraiser.title}</h2>
              <p className="text-gray-500 text-sm mt-1 line-clamp-2">{fundraiser.description}</p>
            </div>
          )}

          {orgName && (
            <div className="mb-4">
              <div className="text-xs text-gray-500 mb-1">Організація</div>
              <div className="text-yellow-400 text-sm font-medium">{orgName}</div>
            </div>
          )}

          {success ? (
            <div className="bg-green-500/10 border border-green-500/20 rounded-xl p-6 text-center">
              <div className="text-3xl mb-2">💛</div>
              <p className="text-green-400 font-semibold">Дякуємо за донат!</p>
              <button
                onClick={() => setSuccess(false)}
                className="mt-4 text-yellow-400 text-sm hover:text-yellow-300"
              >
                Задонатити ще раз
              </button>
            </div>
          ) : (
            <form onSubmit={handleDonate} className="space-y-4">
              {error && (
                <div className="bg-red-500/10 border border-red-500/20 text-red-400 text-sm rounded-lg px-4 py-2">
                  {error}
                </div>
              )}
              <div className="grid grid-cols-3 gap-2">
                {[100, 500, 1000].map(v => (
                  <button
                    key={v}
                    type="button"
                    onClick={() => setAmount(String(v))}
                    className={`py-2 text-sm rounded-lg border transition-colors ${
                      amount === String(v)
                        ? 'bg-yellow-400 text-black border-yellow-400 font-semibold'
                        : 'border-[#2e303a] text-gray-400 hover:border-gray-500'
                    }`}
                  >
                    {v}&nbsp;₴
                  </button>
                ))}
              </div>
              <div className="relative">
                <input
                  type="text"
                  inputMode="numeric"
                  value={formatMoney(amount)}
                  onChange={e => setAmount(unformatMoney(e.target.value))}
                  placeholder="Своя сума"
                  className="w-full bg-[#111218] border border-[#2e303a] rounded-lg px-4 py-3 pr-9 text-white text-sm placeholder-gray-600 focus:outline-none focus:border-yellow-400"
                />
                <span className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 text-sm pointer-events-none">₴</span>
              </div>
              <button
                type="submit"
                disabled={submitting}
                className="w-full bg-yellow-400 text-black font-semibold py-3 rounded-lg hover:bg-yellow-300 transition-colors disabled:opacity-50"
              >
                {submitting ? 'Обробка...' : '💛 Задонатити'}
              </button>
              {!user && (
                <p className="text-gray-600 text-xs text-center">
                  Анонімний донат. <Link to="/login" className="text-yellow-400">Увійдіть</Link>
                  {' '}щоб відстежувати внески.
                </p>
              )}
            </form>
          )}
        </div>
      </div>
    </div>
  )
}
