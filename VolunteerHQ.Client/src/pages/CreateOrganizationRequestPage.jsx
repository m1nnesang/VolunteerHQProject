import { useState, useRef } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import toast from 'react-hot-toast'
import { createOrganizationRequest } from '../api/organizationRequests'
import { useAuth } from '../context/AuthContext'

const NP_API_KEY = '479e5d30c809098cad772e07e4fa438c'

export default function CreateOrganizationRequestPage() {
  const { user, isUser } = useAuth()
  const navigate = useNavigate()
  const [form, setForm] = useState({
    proposedName: '',
    city: '',
    description: '',
    bio: '',
    experience: '',
    skills: '',
    cvFilePath: '',
  })
  const [errors, setErrors] = useState({})
  const [success, setSuccess] = useState(false)
  const [loading, setLoading] = useState(false)

  const [cityQuery, setCityQuery] = useState('')
  const [citySuggestions, setCitySuggestions] = useState([])
  const [cityLoading, setCityLoading] = useState(false)
  const searchTimeout = useRef(null)

  if (!user || !isUser) {
    return (
      <div className="min-h-screen bg-[#111218] flex items-center justify-center px-4">
        <div className="text-center">
          <p className="text-gray-400 mb-4">Тільки звичайні користувачі можуть створювати організації</p>
          <Link to="/login" className="text-yellow-400 hover:text-yellow-300">Увійти</Link>
        </div>
      </div>
    )
  }

  const handleChange = (e) => {
    const { name, value } = e.target
    setForm(prev => ({ ...prev, [name]: value }))
    if (errors[name]) setErrors(prev => ({ ...prev, [name]: '' }))
  }

  const searchCities = async (query) => {
    if (query.length < 2) { setCitySuggestions([]); return }
    setCityLoading(true)
    try {
      const res = await fetch('https://api.novaposhta.ua/v2.0/json/', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          apiKey: NP_API_KEY,
          modelName: 'Address',
          calledMethod: 'getCities',
          methodProperties: { FindByString: query, Limit: 7 }
        })
      })
      const data = await res.json()
      setCitySuggestions(data.data || [])
    } catch {
      setCitySuggestions([])
    } finally {
      setCityLoading(false)
    }
  }

  const handleCityChange = (e) => {
    const val = e.target.value
    setCityQuery(val)
    setForm(prev => ({ ...prev, city: val }))
    if (errors.city) setErrors(prev => ({ ...prev, city: '' }))
    clearTimeout(searchTimeout.current)
    searchTimeout.current = setTimeout(() => searchCities(val), 300)
  }

  const handleCitySelect = (city) => {
    const name = city.Description
    setCityQuery(name)
    setForm(prev => ({ ...prev, city: name }))
    setCitySuggestions([])
    if (errors.city) setErrors(prev => ({ ...prev, city: '' }))
  }

  const validate = () => {
    const e = {}
    if (!form.proposedName.trim()) e.proposedName = 'Назва обов\'язкова'
    else if (form.proposedName.trim().length < 3) e.proposedName = 'Мін. 3 символи'

    if (!form.city.trim()) e.city = 'Місто обов\'язкове'

    if (!form.description.trim()) e.description = 'Опис обов\'язковий'
    else if (form.description.trim().length < 30) e.description = 'Мін. 30 символів'

    if (!form.bio.trim()) e.bio = 'Розкажіть про себе'
    if (!form.experience.trim()) e.experience = 'Опишіть досвід'
    if (!form.skills.trim()) e.skills = 'Вкажіть навички'

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
      await createOrganizationRequest({
        proposedName: form.proposedName.trim(),
        city: form.city.trim(),
        description: form.description.trim(),
        bio: form.bio.trim(),
        experience: form.experience.trim(),
        skills: form.skills.trim(),
        cvFilePath: 'N/A',
      })
      setSuccess(true)
    } catch (err) {
      toast.error(err.response?.data?.error || 'Помилка при подачі заявки')
    } finally {
      setLoading(false)
    }
  }

  const inputClass = (field) =>
    `w-full bg-[#111218] border rounded-lg px-4 py-3 text-white text-sm placeholder-gray-600 focus:outline-none transition-colors ${
      errors[field] ? 'border-red-500 focus:border-red-400' : 'border-[#2e303a] focus:border-yellow-400'
    }`

  if (success) {
    return (
      <div className="min-h-screen bg-[#111218] flex items-center justify-center px-4">
        <div className="max-w-md text-center">
          <div className="w-16 h-16 bg-green-400/20 rounded-full flex items-center justify-center text-green-400 text-3xl mx-auto mb-4">
            ✓
          </div>
          <h1 className="text-white text-2xl font-bold mb-3">Заявку відправлено!</h1>
          <p className="text-gray-400 mb-8">
            Адміністрація розгляне вашу заявку найближчим часом. Ви отримаєте сповіщення про результат.
          </p>
          <button
            onClick={() => navigate('/profile')}
            className="bg-yellow-400 text-black font-semibold px-6 py-3 rounded-lg hover:bg-yellow-300 transition-colors"
          >
            До профілю
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-[#111218]">
      <div className="max-w-2xl mx-auto px-4 py-10">
        <div className="mb-6">
          <h1 className="text-white text-3xl font-bold mb-2">Створення організації</h1>
          <p className="text-gray-500 text-sm">Подайте заявку на створення волонтерської організації</p>
        </div>

        <div className="bg-[#1c1d24] border border-[#2e303a] rounded-2xl p-8">
          <form onSubmit={handleSubmit} className="space-y-5">
            <div className="pb-4 border-b border-[#2e303a]">
              <h2 className="text-white font-semibold mb-4">Про організацію</h2>

              <div className="space-y-4">
                <div>
                  <label className="block text-gray-400 text-sm mb-2">Назва організації</label>
                  <input
                    type="text"
                    name="proposedName"
                    value={form.proposedName}
                    onChange={handleChange}
                    placeholder="ГО «Допомога ЗСУ»"
                    className={inputClass('proposedName')}
                  />
                  {errors.proposedName && <p className="text-red-400 text-xs mt-1">{errors.proposedName}</p>}
                </div>

                <div className="relative">
                  <label className="block text-gray-400 text-sm mb-2">Місто</label>
                  <input
                    type="text"
                    value={cityQuery}
                    onChange={handleCityChange}
                    onBlur={() => setTimeout(() => setCitySuggestions([]), 150)}
                    placeholder="Почніть вводити назву міста..."
                    autoComplete="off"
                    className={inputClass('city')}
                  />
                  {errors.city && <p className="text-red-400 text-xs mt-1">{errors.city}</p>}
                  {(citySuggestions.length > 0 || cityLoading) && (
                    <div className="absolute z-20 w-full mt-1 bg-[#1c1d24] border border-[#2e303a] rounded-lg overflow-hidden shadow-xl">
                      {cityLoading ? (
                        <div className="px-4 py-3 text-gray-500 text-sm">Пошук...</div>
                      ) : (
                        citySuggestions.map(city => (
                          <button
                            key={city.Ref}
                            type="button"
                            onMouseDown={() => handleCitySelect(city)}
                            className="w-full text-left px-4 py-2.5 text-sm text-gray-300 hover:bg-[#2e303a] transition-colors flex items-center justify-between"
                          >
                            <span>{city.Description}</span>
                            <span className="text-gray-600 text-xs">{city.AreaDescription} обл.</span>
                          </button>
                        ))
                      )}
                    </div>
                  )}
                </div>

                <div>
                  <label className="block text-gray-400 text-sm mb-2">Опис діяльності</label>
                  <textarea
                    name="description"
                    value={form.description}
                    onChange={handleChange}
                    placeholder="Чим саме планує займатись організація..."
                    rows={4}
                    className={`${inputClass('description')} resize-none`}
                  />
                  {errors.description && <p className="text-red-400 text-xs mt-1">{errors.description}</p>}
                </div>
              </div>
            </div>

            <div>
              <h2 className="text-white font-semibold mb-4">Про вас (засновника)</h2>
              <div className="space-y-4">
                <div>
                  <label className="block text-gray-400 text-sm mb-2">Про себе</label>
                  <textarea
                    name="bio"
                    value={form.bio}
                    onChange={handleChange}
                    placeholder="Розкажіть про себе..."
                    rows={3}
                    className={`${inputClass('bio')} resize-none`}
                  />
                  {errors.bio && <p className="text-red-400 text-xs mt-1">{errors.bio}</p>}
                </div>

                <div>
                  <label className="block text-gray-400 text-sm mb-2">Навички</label>
                  <input
                    type="text"
                    name="skills"
                    value={form.skills}
                    onChange={handleChange}
                    placeholder="Менеджмент, логістика..."
                    className={inputClass('skills')}
                  />
                  {errors.skills && <p className="text-red-400 text-xs mt-1">{errors.skills}</p>}
                </div>

                <div>
                  <label className="block text-gray-400 text-sm mb-2">Досвід волонтерства</label>
                  <textarea
                    name="experience"
                    value={form.experience}
                    onChange={handleChange}
                    placeholder="Опишіть ваш попередній досвід..."
                    rows={3}
                    className={`${inputClass('experience')} resize-none`}
                  />
                  {errors.experience && <p className="text-red-400 text-xs mt-1">{errors.experience}</p>}
                </div>
              </div>
            </div>

            <div className="flex gap-3 pt-4">
              <button
                type="submit"
                disabled={loading}
                className="bg-yellow-400 text-black font-semibold px-6 py-3 rounded-lg hover:bg-yellow-300 transition-colors disabled:opacity-50"
              >
                {loading ? 'Відправляємо...' : 'Подати заявку'}
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
