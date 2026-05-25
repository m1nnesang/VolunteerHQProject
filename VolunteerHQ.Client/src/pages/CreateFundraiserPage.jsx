import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import toast from 'react-hot-toast'
import { createFundraiser } from '../api/fundraisers'
import { useAuth } from '../context/AuthContext'
import { formatMoney, unformatMoney } from '../utils/money'

const importanceOptions = [
  { value: 'Low', label: 'Низька', color: 'text-gray-400 border-gray-400/30 bg-gray-400/10' },
  { value: 'Medium', label: 'Середня', color: 'text-blue-400 border-blue-400/30 bg-blue-400/10' },
  { value: 'High', label: 'Висока', color: 'text-yellow-400 border-yellow-400/30 bg-yellow-400/10' },
  { value: 'Critical', label: 'Критична', color: 'text-red-400 border-red-400/30 bg-red-400/10' },
]

export default function CreateFundraiserPage() {
  const { isUnit, user } = useAuth()
  const navigate = useNavigate()
  const [form, setForm] = useState({
    title: '',
    description: '',
    totalGoal: '',
    importance: 'Medium',
    deadline: '',
  })
  const [errors, setErrors] = useState({})
  const [loading, setLoading] = useState(false)

  if (!user) {
    return (
      <div className="min-h-screen bg-[#111218] flex items-center justify-center px-4">
        <div className="text-center">
          <p className="text-gray-400 mb-4">Увійдіть як військовий підрозділ</p>
          <Link to="/units/login" className="text-yellow-400 hover:text-yellow-300">Увійти</Link>
        </div>
      </div>
    )
  }

  if (!isUnit) {
    return (
      <div className="min-h-screen bg-[#111218] flex items-center justify-center px-4">
        <div className="text-center">
          <p className="text-gray-400 mb-2">Створювати збори можуть тільки військові підрозділи</p>
          <p className="text-gray-600 text-sm">Ви увійшли як звичайний користувач</p>
        </div>
      </div>
    )
  }

  const handleChange = (e) => {
    const { name, value } = e.target
    setForm(prev => ({ ...prev, [name]: value }))
    if (errors[name]) setErrors(prev => ({ ...prev, [name]: '' }))
  }

  const validate = () => {
    const e = {}
    if (!form.title.trim()) e.title = 'Назва обов\'язкова'
    else if (form.title.trim().length < 5) e.title = 'Мін. 5 символів'

    if (!form.description.trim()) e.description = 'Опис обов\'язковий'
    else if (form.description.trim().length < 20) e.description = 'Мін. 20 символів'

    const goal = parseFloat(form.totalGoal)
    if (!form.totalGoal) e.totalGoal = 'Сума обов\'язкова'
    else if (isNaN(goal) || goal <= 0) e.totalGoal = 'Невірна сума'
    else if (goal < 100) e.totalGoal = 'Мін. 100 ₴'

    if (!form.deadline) e.deadline = 'Дедлайн обов\'язковий'
    else if (new Date(form.deadline) <= new Date()) e.deadline = 'Дата має бути в майбутньому'

    return e
  }

  const handleSubmit = async (event) => {
    event.preventDefault()
    const validationErrors = validate()
    if (Object.keys(validationErrors).length) {
      setErrors(validationErrors)
      return
    }
    setLoading(true)
    try {
      const res = await createFundraiser({
        title: form.title.trim(),
        description: form.description.trim(),
        totalGoal: parseFloat(form.totalGoal),
        importance: form.importance,
        deadline: form.deadline,
      })
      navigate(`/fundraisers/${res.data.id}`)
    } catch (err) {
      toast.error(err.response?.data?.error || 'Помилка при створенні збору')
    } finally {
      setLoading(false)
    }
  }

  const inputClass = (field) =>
    `w-full bg-[#111218] border rounded-lg px-4 py-3 text-white text-sm placeholder-gray-600 focus:outline-none transition-colors ${
      errors[field] ? 'border-red-500 focus:border-red-400' : 'border-[#2e303a] focus:border-yellow-400'
    }`

  return (
    <div className="min-h-screen bg-[#111218]">
      <div className="max-w-2xl mx-auto px-4 py-10">
        <div className="mb-6">
          <h1 className="text-white text-3xl font-bold mb-2">Новий збір</h1>
          <p className="text-gray-500 text-sm">Створіть збір на потреби вашого підрозділу</p>
        </div>

        <div className="bg-[#1c1d24] border border-[#2e303a] rounded-2xl p-8">
          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <label className="block text-gray-400 text-sm mb-2">Назва збору</label>
              <input
                type="text"
                name="title"
                value={form.title}
                onChange={handleChange}
                placeholder="FPV дрони для 3-ї бригади"
                className={inputClass('title')}
              />
              {errors.title && <p className="text-red-400 text-xs mt-1">{errors.title}</p>}
            </div>

            <div>
              <label className="block text-gray-400 text-sm mb-2">Опис</label>
              <textarea
                name="description"
                value={form.description}
                onChange={handleChange}
                placeholder="Детальний опис потреб..."
                rows={5}
                className={`${inputClass('description')} resize-none`}
              />
              {errors.description && <p className="text-red-400 text-xs mt-1">{errors.description}</p>}
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-gray-400 text-sm mb-2">Ціль</label>
                <div className="relative">
                  <input
                    type="text"
                    inputMode="numeric"
                    name="totalGoal"
                    value={formatMoney(form.totalGoal)}
                    onChange={(e) => setForm(prev => ({ ...prev, totalGoal: unformatMoney(e.target.value) }))}
                    placeholder="100 000"
                    className={`${inputClass('totalGoal')} pr-9`}
                  />
                  <span className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 text-sm pointer-events-none">₴</span>
                </div>
                {errors.totalGoal && <p className="text-red-400 text-xs mt-1">{errors.totalGoal}</p>}
              </div>
              <div>
                <label className="block text-gray-400 text-sm mb-2">Дедлайн</label>
                <input
                  type="date"
                  name="deadline"
                  value={form.deadline}
                  onChange={handleChange}
                  className={inputClass('deadline')}
                />
                {errors.deadline && <p className="text-red-400 text-xs mt-1">{errors.deadline}</p>}
              </div>
            </div>

            <div>
              <label className="block text-gray-400 text-sm mb-2">Важливість</label>
              <div className="grid grid-cols-4 gap-2">
                {importanceOptions.map(opt => (
                  <button
                    key={opt.value}
                    type="button"
                    onClick={() => setForm(prev => ({ ...prev, importance: opt.value }))}
                    className={`py-2 text-sm rounded-lg border transition-colors ${
                      form.importance === opt.value
                        ? opt.color
                        : 'border-[#2e303a] text-gray-500 hover:border-gray-500'
                    }`}
                  >
                    {opt.label}
                  </button>
                ))}
              </div>
            </div>

            <div className="flex gap-3 pt-4">
              <button
                type="submit"
                disabled={loading}
                className="bg-yellow-400 text-black font-semibold px-6 py-3 rounded-lg hover:bg-yellow-300 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {loading ? 'Створюємо...' : 'Створити збір'}
              </button>
              <button
                type="button"
                onClick={() => navigate(-1)}
                className="border border-[#2e303a] text-gray-400 px-6 py-3 rounded-lg hover:border-gray-500 transition-colors"
              >
                Скасувати
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  )
}
