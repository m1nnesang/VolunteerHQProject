import { useState } from 'react'
import toast from 'react-hot-toast'
import { assignOrganization } from '../api/fundraisers'
import { useAuth } from '../context/AuthContext'

export default function AssignOrgPanel({ fundraiserId, onAssigned }) {
  const { managedOrgId } = useAuth()
  const [loading, setLoading] = useState(false)
  const [open, setOpen] = useState(false)

  if (!managedOrgId) return null

  const handleAssign = async () => {
    setLoading(true)
    try {
      await assignOrganization(fundraiserId, managedOrgId)
      toast.success('Організацію підключено!')
      setOpen(false)
      onAssigned?.()
    } catch (err) {
      toast.error(err.response?.data?.error || 'Помилка. Можливо, у вас немає прав')
    } finally {
      setLoading(false)
    }
  }

  if (!open) {
    return (
      <button
        onClick={() => setOpen(true)}
        className="text-sm border border-[#2e303a] text-gray-400 px-4 py-2 rounded-lg hover:border-yellow-400 hover:text-yellow-400 transition-colors w-full"
      >
        🤝 Підключити свою організацію
      </button>
    )
  }

  return (
    <div className="bg-[#111218] border border-[#2e303a] rounded-xl p-4 space-y-3">
      <div className="text-sm text-gray-300 font-medium">Підключити організацію</div>
      <p className="text-xs text-gray-500">
        Буде підключено вашу організацію до цього збору.
      </p>
      <div className="flex gap-2">
        <button
          onClick={handleAssign}
          disabled={loading}
          className="flex-1 bg-yellow-400 text-black text-sm font-semibold py-2 rounded-lg hover:bg-yellow-300 transition-colors disabled:opacity-50"
        >
          {loading ? 'Підключаємо...' : 'Підключити'}
        </button>
        <button
          onClick={() => setOpen(false)}
          className="border border-[#2e303a] text-gray-500 text-sm px-3 py-2 rounded-lg hover:border-gray-500 transition-colors"
        >
          Скасувати
        </button>
      </div>
    </div>
  )
}
